namespace CodeEditor.MCP.Services;

public interface IDotNetService
{
    Task<string> BuildProject(string relativePath);
    Task<string> BuildSolution(string relativePath);
}
