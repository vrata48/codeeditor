using System.ComponentModel;
using System.Linq;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Extensions;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
public static class BuildTools
{
    [McpServerTool]
    [Description("Build C# project.")]
    public static async Task<string> BuildProject(
        IDotNetService service,
        [Description("Path to .csproj file.")] string path)
    {
        var result = await service.BuildProjectAsync(path);
        var json = result.ToFormattedJson();
        
        if (!result.Success && result.ParsedErrors.Any())
        {
            var formattedErrors = result.ParsedErrors.Select(e => 
                $"{e.Severity} {e.ErrorCode}: {e.Message}\n  {e.File}({e.Line},{e.Column})");
            json += "\n\nParsed Errors:\n" + string.Join("\n", formattedErrors);
        }
        
        return json;
    }

    [McpServerTool]
    [Description("Build C# solution.")]
    public static async Task<string> BuildSolution(
        IDotNetService service,
        [Description("Path to .sln file.")] string path)
    {
        var result = await service.BuildSolutionAsync(path);
        return result.ToFormattedJson();
    }

    [McpServerTool]
    [Description("Clean C# project.")]
    public static async Task<string> CleanProject(
        IDotNetService service,
        [Description("Path to .csproj file.")] string path)
    {
        var result = await service.CleanProjectAsync(path);
        return result.ToFormattedJson();
    }

    [McpServerTool]
    [Description("Clean C# solution.")]
    public static async Task<string> CleanSolution(
        IDotNetService service,
        [Description("Path to .sln file.")] string path)
    {
        var result = await service.CleanSolutionAsync(path);
        return result.ToFormattedJson();
    }

    [McpServerTool]
    [Description("Restore NuGet packages for project or solution.")]
    public static async Task<string> RestorePackages(
        IDotNetService service,
        [Description("Path to .csproj or .sln file.")]
        string path)
    {
        var result = await service.RestorePackagesAsync(path);
        var json = result.ToFormattedJson();
        
        if (!result.Success && result.ParsedErrors.Any())
        {
            var formattedErrors = result.ParsedErrors.Select(e => 
                $"{e.Severity} {e.ErrorCode}: {e.Message}\n  {e.File}({e.Line},{e.Column})");
            json += "\n\nParsed Errors:\n" + string.Join("\n", formattedErrors);
        }
        
        return json;
    }

    [McpServerTool]
    [Description("Run tests for a project.")]
    public static async Task<string> RunTests(
        IDotNetService service,
        [Description("Path to test .csproj file.")]
        string path)
    {
        var result = await service.RunTestsAsync(path);
        var json = result.ToFormattedJson();
        
        if (!result.Success)
        {
            // Add failed test details if any
            if (result.FailedTests.Any())
            {
                json += "\n\nFailed Tests:\n";
                foreach (var test in result.FailedTests)
                {
                    json += $"- {test.TestName} ({test.ClassName})\n";
                    json += $"  Error: {test.ErrorMessage}\n";
                    if (!string.IsNullOrEmpty(test.StackTrace))
                    {
                        json += $"  Stack Trace: {test.StackTrace}\n";
                    }
                    json += "\n";
                }
            }
        }
        
        return "Test Result:\n" + json;
    }

    [McpServerTool]
    [Description("Run filtered tests for a project.")]
    public static async Task<string> RunTestsFiltered(
        IDotNetService service,
        [Description("Path to test .csproj file.")]
        string path,
        [Description("Test filter expression (e.g., 'ClassName=MyTests' or 'Method~Integration')")]
        string filter)
    {
        var result = await service.RunTestsAsync(path, filter);
        var json = result.ToFormattedJson();
        
        if (!result.Success)
        {
            // Add failed test details if any
            if (result.FailedTests.Any())
            {
                json += "\n\nFailed Tests:\n";
                foreach (var test in result.FailedTests)
                {
                    json += $"- {test.TestName} ({test.ClassName})\n";
                    json += $"  Error: {test.ErrorMessage}\n";
                    if (!string.IsNullOrEmpty(test.StackTrace))
                    {
                        json += $"  Stack Trace: {test.StackTrace}\n";
                    }
                    json += "\n";
                }
            }
        }
        
        return "Test Result:\n" + json;
    }
[McpServerTool]
    [Description("Publish C# project.")]
    public static async Task<string> PublishProject(
        IDotNetService service,
        [Description("Path to .csproj file.")] string path,
        [Description("Output directory path (optional).")]
        string? outputPath = null)
    {
        var result = await service.PublishProjectAsync(path, outputPath);
        return result.ToFormattedJson();
    } }
