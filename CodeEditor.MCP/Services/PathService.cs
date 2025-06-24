using Ignore;

namespace CodeEditor.MCP.Services;

public class PathService : IPathService
{
    private readonly string _baseDirectory;
    private readonly List<string> _gitignorePatterns = new();
    private readonly Ignore.Ignore _ignore = new();

    public PathService(string baseDirectory)
    {
        _baseDirectory = Path.GetFullPath(baseDirectory);
        
        // Always ignore .git directory
        _gitignorePatterns.Add(".git/");
        _ignore.Add(".git/");
        
        // Load .gitignore files from current directory up to root
        LoadGitignoreFilesUpward(_baseDirectory);
    }

    private void LoadGitignoreFilesUpward(string startDirectory)
    {
        var currentDirectory = Path.GetFullPath(startDirectory);
        var visitedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        while (!string.IsNullOrEmpty(currentDirectory) && !visitedDirectories.Contains(currentDirectory))
        {
            visitedDirectories.Add(currentDirectory);
            
            var gitignorePath = Path.Combine(currentDirectory, ".gitignore");
            if (File.Exists(gitignorePath))
            {
                LoadGitignoreFile(gitignorePath);
            }
            
            // Move up to parent directory
            var parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            if (parentDirectory == currentDirectory)
            {
                // We've reached the root and can't go further up
                break;
            }
            currentDirectory = parentDirectory;
        }
    }

    private void LoadGitignoreFile(string gitignorePath)
    {
        try
        {
            var gitignoreContent = File.ReadAllLines(gitignorePath);
            foreach (var line in gitignoreContent)
            {
                var trimmedLine = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith("#"))
                {
                    try
                    {
                        _gitignorePatterns.Add(trimmedLine);
                        _ignore.Add(trimmedLine);
                    }
                    catch
                    {
                        // Skip malformed patterns but continue with others
                    }
                }
            }
        }
        catch
        {
            // If we can't read a .gitignore file, continue with others
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
        
        // Always ignore .git directory and its contents
        if (normalizedPath.StartsWith(".git/") || normalizedPath == ".git")
            return true;
        
        bool shouldIgnore = false;
        bool hasNegationMatch = false;
        
        // Check each gitignore pattern manually for more control
        // Process all patterns to handle negation properly
        foreach (var pattern in _gitignorePatterns)
        {
            var trimmedPattern = pattern.Trim();
            if (string.IsNullOrEmpty(trimmedPattern))
                continue;
                
            if (trimmedPattern.StartsWith("!"))
            {
                // Negation pattern - if it matches, don't ignore
                var negationPattern = trimmedPattern.Substring(1);
                if (MatchesGitignorePattern(normalizedPath, negationPattern))
                {
                    hasNegationMatch = true;
                    shouldIgnore = false; // Override any previous ignore decision
                }
            }
            else
            {
                // Normal ignore pattern - only set to ignore if no negation match yet
                if (MatchesGitignorePattern(normalizedPath, trimmedPattern))
                {
                    if (!hasNegationMatch) // Only set ignore if no negation has been found
                        shouldIgnore = true;
                }
            }
        }
        
        // If we found a negation match, respect it regardless of other patterns
        if (hasNegationMatch)
            return false;
        
        // If our manual processing found a match, use it
        if (shouldIgnore)
            return true;
        
        // Use the Ignore library as fallback for complex patterns we might have missed
        try
        {
            return _ignore.IsIgnored(normalizedPath);
        }
        catch
        {
            return false;
        }
    }     private bool MatchesGitignorePattern(string path, string pattern)
    {
        // Handle basic gitignore patterns
        pattern = pattern.Trim();
        
        if (string.IsNullOrEmpty(pattern) || pattern.StartsWith("#"))
            return false;
            
        // Handle directory patterns (ending with /)
        if (pattern.EndsWith("/"))
        {
            var dirPattern = pattern.TrimEnd('/');
            // Check if the path starts with this directory name
            return path.StartsWith(dirPattern + "/") || path == dirPattern || path == dirPattern + "/";
        }
        
        // Handle file extension patterns (*.ext)
        if (pattern.StartsWith("*."))
        {
            var extension = pattern.Substring(1); // Remove the *
            return path.EndsWith(extension);
        }
        
        // Handle exact file name matches
        if (!pattern.Contains('/'))
        {
            var fileName = Path.GetFileName(path);
            return fileName == pattern;
        }
        
        // Handle path patterns
        return path.Contains(pattern) || path.StartsWith(pattern);
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
        return fullPath.StartsWith(_baseDirectory + Path.DirectorySeparatorChar) || fullPath.Equals(_baseDirectory, StringComparison.OrdinalIgnoreCase);
    }

    public string GetNamespaceFromPath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return "DefaultNamespace";
        // Get directory path and convert to namespace format
        var directory = Path.GetDirectoryName(relativePath) ?? "";
        // Replace path separators with dots and remove any invalid characters
        var namespaceName = directory.Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.').Trim('.');
        // If empty, use a default namespace
        if (string.IsNullOrEmpty(namespaceName))
            return "DefaultNamespace";
        return namespaceName;
    }
public string GetRelativePath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return "";
            
        // Ensure the full path is normalized
        var normalizedFullPath = Path.GetFullPath(fullPath);
        var normalizedBasePath = Path.GetFullPath(_baseDirectory);
        
        // Get the relative path
        var relativePath = Path.GetRelativePath(normalizedBasePath, normalizedFullPath);
        
        // Normalize to forward slashes
        return relativePath.Replace('\\', '/');
    }     public bool ShouldIgnoreDirectory(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return false;
        // Normalize path separators to forward slashes for gitignore compatibility
        var normalizedPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
        // Ensure directory path ends with slash for gitignore matching
        if (!normalizedPath.EndsWith("/"))
            normalizedPath += "/";
        // Check .gitignore rules first
        if (_ignore.IsIgnored(normalizedPath))
            return true;
        // Check common ignore directories as fallback
        var dirName = Path.GetFileName(relativePath.TrimEnd('/', '\\'));
        var commonIgnoreDirs = new[]
        {
            "bin",
            "obj",
            "node_modules",
            "packages",
            ".git",
            ".vs",
            ".vscode",
            ".idea"
        };
        return commonIgnoreDirs.Contains(dirName.ToLowerInvariant());
    }

    public bool ShouldIgnoreFile(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return false;
        // Normalize path separators to forward slashes for gitignore compatibility
        var normalizedPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
        // Check .gitignore rules first
        if (_ignore.IsIgnored(normalizedPath))
            return true;
        // Check common ignore file extensions as fallback
        var extension = Path.GetExtension(relativePath).ToLowerInvariant();
        var ignoreExtensions = new[]
        {
            ".dll",
            ".exe",
            ".pdb",
            ".cache",
            ".tmp"
        };
        // Check if file is in common ignore directories
        var ignorePaths = new[]
        {
            "bin",
            "obj",
            ".git",
            "node_modules",
            "packages"
        };
        var hasIgnorePath = ignorePaths.Any(ignore => normalizedPath.Contains($"/{ignore}/") || normalizedPath.StartsWith($"{ignore}/"));
        return ignoreExtensions.Contains(extension) || hasIgnorePath;
    }

    public bool ShouldIgnoreFileByPath(string fullPath)
    {
        var relativePath = GetRelativePath(fullPath);
        return ShouldIgnoreFile(relativePath);
    }

    public bool ShouldIgnoreDirectoryByPath(string fullPath)
    {
        var relativePath = GetRelativePath(fullPath);
        return ShouldIgnoreDirectory(relativePath);
    }
}
