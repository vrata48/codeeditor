using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services;

public interface IDotNetService
{
    Task<BuildResult> BuildProjectAsync(string relativePath);
    Task<BuildResult> BuildSolutionAsync(string relativePath);
    Task<BuildResult> CleanProjectAsync(string relativePath);
    Task<BuildResult> CleanSolutionAsync(string relativePath);
    Task<BuildResult> RestorePackagesAsync(string relativePath);
    Task<TestResult> RunTestsAsync(string relativePath);
    Task<TestResult> RunTestsAsync(string relativePath, string filter);
    Task<BuildResult> PublishProjectAsync(string relativePath, string? outputPath = null);
}
