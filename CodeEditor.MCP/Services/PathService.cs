using Ignore;

namespace CodeEditor.MCP.Services;

public class PathService : IPathService
{
    private readonly string _baseDirectory;
    private readonly Ignore.Ignore _ignore = new();

    public PathService(string baseDirectory)
    {
        _baseDirectory = Path.GetFullPath(baseDirectory);
        
        // Load .gitignore if it exists
        var gitignorePath = Path.Combine(_baseDirectory, ".gitignore");
        if (File.Exists(gitignorePath))
        {
            try
            {
                var gitignoreContent = File.ReadAllLines(gitignorePath);
                _ignore.Add(gitignoreContent);
            }
            catch
            {
                // If we can't read the .gitignore file, continue with empty rules
            }
        }
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
        if (string.IsNullOrEmpty(relativePath))
            return false;
            
        // Normalize path separators to forward slashes for gitignore compatibility
        var normalizedPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
        return _ignore.IsIgnored(normalizedPath);
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
public string GetNamespaceFromPath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return "DefaultNamespace";

        // Get directory path and convert to namespace format
        var directory = Path.GetDirectoryName(relativePath) ?? "";
        
        // Replace path separators with dots and remove any invalid characters
        var namespaceName = directory
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.')
            .Trim('.');

        // If empty, use a default namespace
        if (string.IsNullOrEmpty(namespaceName))
            return "DefaultNamespace";

        return namespaceName;
    } public string GetRelativePath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return "";

        // Ensure the full path is normalized
        var normalizedFullPath = Path.GetFullPath(fullPath);
        var normalizedBasePath = Path.GetFullPath(_baseDirectory);

        // Get the relative path
        var relativePath = Path.GetRelativePath(normalizedBasePath, normalizedFullPath);
        
        return relativePath;
    } }
