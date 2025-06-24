namespace CodeEditor.MCP.Services;

public interface IFileFilterService
{
    string? GlobalFilter { get; }
    bool ShouldInclude(string relativePath);
    IEnumerable<string> FilterFiles(IEnumerable<string> relativePaths);
}