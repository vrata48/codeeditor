namespace CodeEditor.MCP.Services;

public interface IPathService
{
    string GetFullPath(string relativePath);
    string GetBaseDirectory();
    void SetBaseDirectory(string baseDirectory);
    bool ShouldIgnore(string relativePath);
    IEnumerable<string> FilterIgnored(IEnumerable<string> relativePaths);
    string GetNamespaceFromPath(string relativePath);
    string GetRelativePath(string fullPath);
    bool ShouldIgnoreDirectory(string relativePath);
    bool ShouldIgnoreFile(string relativePath);
    bool ShouldIgnoreFileByPath(string fullPath);
    bool ShouldIgnoreDirectoryByPath(string fullPath);
    bool MatchesFilter(string relativePath, string? filterPatterns);
    IEnumerable<string> FilterByPatterns(IEnumerable<string> relativePaths, string? filterPatterns);
}