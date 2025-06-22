using CliWrap;
using CliWrap.Buffered;
using CodeEditor.MCP.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CodeEditor.MCP.Services;

public class DotNetService(IPathService pathService) : IDotNetService
{
    public async Task<BuildResult> BuildProjectAsync(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        return await ExecuteBuildCommandAsync("build", fullPath);
    }

    public async Task<BuildResult> BuildSolutionAsync(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        return await ExecuteBuildCommandAsync("build", fullPath);
    }

    public async Task<BuildResult> CleanProjectAsync(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        return await ExecuteBuildCommandAsync("clean", fullPath);
    }

    public async Task<BuildResult> CleanSolutionAsync(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        return await ExecuteBuildCommandAsync("clean", fullPath);
    }

    public async Task<BuildResult> RestorePackagesAsync(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        return await ExecuteBuildCommandAsync("restore", fullPath);
    }

    public async Task<TestResult> RunTestsAsync(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        return await ExecuteTestCommandAsync(fullPath);
    }

    public async Task<TestResult> RunTestsAsync(string relativePath, string filter)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        return await ExecuteTestCommandAsync(fullPath, filter);
    }
public async Task<BuildResult> PublishProjectAsync(string relativePath, string? outputPath = null)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var args = $"publish \"{fullPath}\"";
        if (!string.IsNullOrEmpty(outputPath))
        {
            args += $" --output \"{outputPath}\"";
        }

        return await ExecuteCommandInternalAsync(args);
    } 
private async Task<BuildResult> ExecuteBuildCommandAsync(string command, string path)
    {
        var args = $"{command} \"{path}\" --verbosity normal --no-restore";
        return await ExecuteCommandInternalAsync(args);
    }     private async Task<TestResult> ExecuteTestCommandAsync(string path, string? filter = null)
    {
        var args = $"test \"{path}\" --verbosity normal --logger console --no-restore";
        if (!string.IsNullOrEmpty(filter))
        {
            args += $" --filter \"{filter}\"";
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments(args)
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            stopwatch.Stop();

            var testResult = new TestResult
            {
                Success = result.ExitCode == 0,
                Output = result.StandardOutput,
                Errors = result.StandardError,
                ExitCode = result.ExitCode,
                Duration = stopwatch.Elapsed
            };

            ParseTestResults(testResult);
            return testResult;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new TestResult
            {
                Success = false,
                Output = string.Empty,
                Errors = ex.Message,
                ExitCode = -1,
                Duration = stopwatch.Elapsed
            };
        }
    } private async Task<BuildResult> ExecuteCommandInternalAsync(string arguments)
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments(arguments)
                .WithWorkingDirectory(pathService.GetBaseDirectory())
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            var buildResult = new BuildResult
            {
                Success = result.ExitCode == 0,
                Output = result.StandardOutput,
                Errors = result.StandardError,
                ExitCode = result.ExitCode
            };

            ParseBuildErrors(buildResult);
            return buildResult;
        }
        catch (Exception ex)
        {
            return new BuildResult
            {
                Success = false,
                Output = string.Empty,
                Errors = ex.Message,
                ExitCode = -1
            };
        }
    } 
    private static void ParseBuildErrors(BuildResult result)
    {
        if (string.IsNullOrEmpty(result.Errors) && string.IsNullOrEmpty(result.Output))
            return;

        var content = result.Errors + Environment.NewLine + result.Output;
        var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Regex pattern for MSBuild error/warning format
        var pattern = @"^(.+?)\((\d+),(\d+)\):\s*(error|warning)\s*([A-Z0-9]+):\s*(.+)$";
        var regex = new Regex(pattern, RegexOptions.Multiline);

        foreach (var line in lines)
        {
            var match = regex.Match(line);
            if (match.Success)
            {
                result.ParsedErrors.Add(new BuildError
                {
                    File = match.Groups[1].Value,
                    Line = int.Parse(match.Groups[2].Value),
                    Column = int.Parse(match.Groups[3].Value),
                    Severity = match.Groups[4].Value,
                    ErrorCode = match.Groups[5].Value,
                    Message = match.Groups[6].Value
                });
            }
        }
    }

    private static void ParseTestResults(TestResult result)
    {
        if (string.IsNullOrEmpty(result.Output))
            return;

        // Parse test summary like "Passed: 5, Failed: 2, Skipped: 1"
        var summaryPattern = @"Passed:\s*(\d+).*?Failed:\s*(\d+).*?Skipped:\s*(\d+)";
        var summaryMatch = Regex.Match(result.Output, summaryPattern);

        if (summaryMatch.Success)
        {
            result.TestsPassed = int.Parse(summaryMatch.Groups[1].Value);
            result.TestsFailed = int.Parse(summaryMatch.Groups[2].Value);
            result.TestsSkipped = int.Parse(summaryMatch.Groups[3].Value);
            result.TotalTests = result.TestsPassed + result.TestsFailed + result.TestsSkipped;
        }

        // Parse failed test details
        var failedTestPattern = @"Failed\s+(.+?)\s+\[(\d+).*?\]\s*(.+?)(?=\s*at\s|\s*Stack|\s*Failed|\s*$)";
        var failedMatches = Regex.Matches(result.Output, failedTestPattern,
            RegexOptions.Multiline | RegexOptions.Singleline);

        foreach (Match match in failedMatches)
        {
            result.FailedTests.Add(new FailedTest
            {
                TestName = match.Groups[1].Value.Trim(),
                ErrorMessage = match.Groups[3].Value.Trim(),
                ClassName = ExtractClassName(match.Groups[1].Value)
            });
        }
    }

    private static string ExtractClassName(string fullTestName)
    {
        var lastDot = fullTestName.LastIndexOf('.');
        return lastDot >= 0 ? fullTestName[..lastDot] : fullTestName;
    }
}