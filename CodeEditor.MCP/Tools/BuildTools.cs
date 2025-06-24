using System.ComponentModel;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Aspects;
using CodeEditor.MCP.Models;
using CodeEditor.MCP.Extensions;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
[ToolLoggingAspect] // Apply logging aspect to all methods in this class
public static class BuildTools
{
    [McpServerTool]
    [Description("Build C# project.")]
    public static async Task<string> BuildProject(
        IDotNetService service,
        [Description("Path to .csproj file.")] string path)
    {
        var result = await service.BuildProjectAsync(path);
        return FormatBuildResult(result);
    }
    
    [McpServerTool]
    [Description("Build C# solution.")]
    public static async Task<string> BuildSolution(
        IDotNetService service,
        [Description("Path to .sln file.")] string path)
    {
        var result = await service.BuildSolutionAsync(path);
        return FormatBuildResult(result);
    }
    
    [McpServerTool]
    [Description("Clean C# project.")]
    public static async Task<string> CleanProject(
        IDotNetService service,
        [Description("Path to .csproj file.")] string path)
    {
        var result = await service.CleanProjectAsync(path);
        return FormatBuildResult(result);
    }
    
    [McpServerTool]
    [Description("Clean C# solution.")]
    public static async Task<string> CleanSolution(
        IDotNetService service,
        [Description("Path to .sln file.")] string path)
    {
        var result = await service.CleanSolutionAsync(path);
        return FormatBuildResult(result);
    }
    
    [McpServerTool]
    [Description("Restore NuGet packages for project or solution.")]
    public static async Task<string> RestorePackages(
        IDotNetService service,
        [Description("Path to .csproj or .sln file.")] string path)
    {
        var result = await service.RestorePackagesAsync(path);
        return FormatBuildResult(result);
    }
    
    [McpServerTool]
    [Description("Run tests for a project.")]
    public static async Task<string> RunTests(
        IDotNetService service,
        [Description("Path to test .csproj file.")] string path)
    {
        var result = await service.RunTestsAsync(path);
        return FormatTestResult(result);
    }
    
    [McpServerTool]
    [Description("Run filtered tests for a project.")]
    public static async Task<string> RunTestsFiltered(
        IDotNetService service,
        [Description("Path to test .csproj file.")] string path,
        [Description("Test filter expression (e.g., 'ClassName=MyTests' or 'Method~Integration')")] string filter)
    {
        var result = await service.RunTestsAsync(path, filter);
        return FormatTestResult(result);
    }
    
    [McpServerTool]
    [Description("Publish C# project.")]
    public static async Task<string> PublishProject(
        IDotNetService service,
        [Description("Path to .csproj file.")] string path,
        [Description("Output directory path (optional).")] string? outputPath = null)
    {
        var result = await service.PublishProjectAsync(path, outputPath);
        return FormatBuildResult(result);
    }

    private static string FormatBuildResult(BuildResult result)
    {
        return result.ToFormattedJson();
    }

    private static string FormatTestResult(TestResult result)
    {
        return result.ToFormattedJson();
    }
}
