namespace CodeEditor.MCP.Services;

public class DotNetService(IPathService pathService) : IDotNetService
{
    public Task<string> BuildProject(string relativePath)
    {
        throw new NotImplementedException();
    }

    public Task<string> BuildSolution(string relativePath)
    {
        throw new NotImplementedException();
    }
}
