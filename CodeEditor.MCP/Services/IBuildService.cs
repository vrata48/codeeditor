namespace CodeEditor.MCP.Services;

public interface IBuildService
{
    Task<string> BuildProject(string relativePath);
    Task<string> BuildSolution(string relativePath);
}
