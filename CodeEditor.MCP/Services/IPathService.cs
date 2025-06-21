namespace CodeEditor.MCP.Services;

public interface IPathService
{
    string GetFullPath(string relativePath);
    string GetBaseDirectory();
    bool ShouldIgnore(string relativePath);
    IEnumerable<string> FilterIgnored(IEnumerable<string> relativePaths);
}
