using System.ComponentModel;
using CodeEditor.MCP.Services;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
public static class BuildTools
{
    [McpServerTool]
    [Description("Build C# project.")]
    public static async Task<string> BuildProject(
        IBuildService service,
        [Description("Path to .csproj file.")] string path)
    {
        return await service.BuildProject(path);
    }
    
    [McpServerTool]
    [Description("Build C# solution.")]
    public static async Task<string> BuildSolution(
        IBuildService service,
        [Description("Path to .sln file.")] string path)
    {
        return await service.BuildSolution(path);
    }
}
