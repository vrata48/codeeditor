namespace CodeEditor.MCP.Services;

public interface IPathService
{
    string GetFullPath(string relativePath);
    string GetBaseDirectory();
    bool ShouldIgnore(string relativePath);
    IEnumerable<string> FilterIgnored(IEnumerable<string> relativePaths);
    string GetNamespaceFromPath(string relativePath);
string GetRelativePath(string fullPath); bool ShouldIgnoreDirectory(string relativePath); bool ShouldIgnoreFile(string relativePath); bool ShouldIgnoreFileByPath(string fullPath); bool ShouldIgnoreDirectoryByPath(string fullPath); }
