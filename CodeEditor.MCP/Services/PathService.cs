namespace CodeEditor.MCP.Services;

public class PathService(string baseDirectory) : IPathService
{
    private readonly string _baseDirectory = Path.GetFullPath(baseDirectory);
    private readonly Ignore.Ignore _ignore = CreateIgnore(baseDirectory);

    private static Ignore.Ignore CreateIgnore(string baseDirectory)
    {
        var ignore = new Ignore.Ignore();
        var gitignorePath = Path.Combine(baseDirectory, ".gitignore");
        if (File.Exists(gitignorePath))
            ignore.Add(gitignorePath);
        return ignore;
    }

    public string GetFullPath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return _baseDirectory;

        var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, relativePath));
        
        if (!IsWithinBaseDirectory(relativePath))
            throw new UnauthorizedAccessException($"Path '{relativePath}' attempts to access outside the base directory");
            
        return fullPath;
    }

    public string GetBaseDirectory()
    {
        return _baseDirectory;
    }

    public bool ShouldIgnore(string relativePath)
    {
        return _ignore.IsIgnored(relativePath);
    }

    public IEnumerable<string> FilterIgnored(IEnumerable<string> relativePaths)
    {
        return relativePaths.Where(p => !ShouldIgnore(p));
    }

    private bool IsWithinBaseDirectory(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return true;
            
        var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, relativePath));
        return fullPath.StartsWith(_baseDirectory + Path.DirectorySeparatorChar) || 
               fullPath.Equals(_baseDirectory, StringComparison.OrdinalIgnoreCase);
    }
}
