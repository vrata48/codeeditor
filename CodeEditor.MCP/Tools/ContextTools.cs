using System.ComponentModel;
using System.Text.RegularExpressions;
using CodeEditor.MCP.Services;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
public static class ContextTools
{
    [McpServerTool]
    [Description("Read specific line ranges from files")]
    public static async Task<string> ReadFileLines(
        IPathService pathService,
        [Description("Relative path to file")] string path,
        [Description("Starting line number (1-based)")] int startLine,
        [Description("Ending line number (1-based, inclusive)")] int endLine)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        
        if (startLine < 1)
            throw new ArgumentException("Start line must be >= 1", nameof(startLine));
        
        if (endLine < startLine)
            throw new ArgumentException("End line must be >= start line", nameof(endLine));

        var fullPath = pathService.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {path}");

        try
        {
            var allLines = await File.ReadAllLinesAsync(fullPath);
            
            // Adjust for 0-based indexing
            var startIndex = startLine - 1;
            var endIndex = Math.Min(endLine - 1, allLines.Length - 1);
            
            if (startIndex >= allLines.Length)
                return $"// File only has {allLines.Length} lines, cannot read from line {startLine}";
            
            var selectedLines = allLines
                .Skip(startIndex)
                .Take(endIndex - startIndex + 1)
                .ToArray();
            
            var result = string.Join(Environment.NewLine, selectedLines);
            
            // Add context information
            var header = $"// Lines {startLine}-{Math.Min(endLine, allLines.Length)} of {allLines.Length} total lines from {path}";
            return $"{header}{Environment.NewLine}{result}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error reading lines from {path}: {ex.Message}", ex);
        }
    }

    [McpServerTool]
    [Description("Read lines around a specific line number")]
    public static async Task<string> ReadAroundLine(
        IPathService pathService,
        [Description("Relative path to file")] string path,
        [Description("Line number to center on (1-based)")] int centerLine,
        [Description("Number of lines to include before and after center line")] int contextLines = 5)
    {
        var startLine = Math.Max(1, centerLine - contextLines);
        var endLine = centerLine + contextLines;
        
        return await ReadFileLines(pathService, path, startLine, endLine);
    }

    [McpServerTool]
    [Description("Search for text in files with surrounding context")]
    public static async Task<string> SearchFilesWithContext(
        IPathService pathService,
        [Description("Text to search for")] string text,
        [Description("Path to search in (default: root)")] string path = "",
        [Description("Number of lines to include before and after match")] int contextLines = 3,
        [Description("File pattern filter (e.g., \"*.cs\")")] string filePattern = "*",
        [Description("Maximum number of results to return")] int maxResults = 20)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Search text cannot be null or empty", nameof(text));

        var searchPath = string.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : pathService.GetFullPath(path);
        
        if (!Directory.Exists(searchPath) && !File.Exists(searchPath))
            throw new DirectoryNotFoundException($"Path not found: {searchPath}");

        var results = new List<SearchResult>();
        var files = GetFilesToSearch(searchPath, filePattern, pathService);

        foreach (var file in files.Take(100)) // Limit files to search
        {
            try
            {
                var matches = await SearchFileWithContextAsync(file, text, contextLines, pathService);
                results.AddRange(matches);
                
                if (results.Count >= maxResults)
                    break;
            }
            catch (Exception)
            {
                // Skip files that can't be read
                continue;
            }
        }

        return FormatSearchResults(results.Take(maxResults).ToList(), text);
    }

    [McpServerTool]
    [Description("Get method signatures from C# files without implementation details")]
    public static async Task<string> GetMethodSignatures(
        IPathService pathService,
        [Description("Relative path to .cs file")] string path,
        [Description("Optional: specific class name to analyze")] string? className = null,
        [Description("Include property signatures")] bool includeProperties = true)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("File must be a C# file (.cs)", nameof(path));

        var fullPath = pathService.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {path}");

        try
        {
            var content = await File.ReadAllTextAsync(fullPath);
            var lines = content.Split('\n');
            
            var methods = ExtractMethodSignatures(content, lines, className);
            var properties = includeProperties ? ExtractPropertySignatures(content, lines, className) : new List<PropertySignature>();
            
            return FormatSignatureOutput(path, methods, properties, className);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error analyzing method signatures in {path}: {ex.Message}", ex);
        }
    }

    [McpServerTool]
    [Description("Generate structured directory overview with filtering")]
    public static async Task<string> FileTreeSummary(
        IPathService pathService,
        [Description("Path to analyze (default: root)")] string path = "",
        [Description("Maximum directory depth to traverse")] int maxDepth = 3,
        [Description("File extensions to include (e.g., \"cs,json,md\")")] string fileTypes = "",
        [Description("Include hidden files and directories")] bool includeHidden = false,
        [Description("Include file sizes and line counts")] bool includeDetails = true,
        [Description("Sort files by: name, size, modified, extension")] string sortBy = "name")
    {
        var searchPath = string.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : pathService.GetFullPath(path);
        
        if (!Directory.Exists(searchPath))
            throw new DirectoryNotFoundException($"Directory not found: {searchPath}");

        var allowedExtensions = ParseFileTypes(fileTypes);
        var dirInfo = await AnalyzeDirectoryAsync(searchPath, "", 0, maxDepth, allowedExtensions, includeHidden, includeDetails, pathService);
        
        return FormatDirectoryTree(dirInfo, includeDetails, sortBy);
    }

    // Helper classes and methods
    private class SearchResult
    {
        public string FilePath { get; set; } = "";
        public int LineNumber { get; set; }
        public string MatchLine { get; set; } = "";
        public List<string> ContextBefore { get; set; } = new();
        public List<string> ContextAfter { get; set; } = new();
        public string MatchedText { get; set; } = "";
    }

    private class MethodSignature
    {
        public string Name { get; set; } = "";
        public string ReturnType { get; set; } = "";
        public List<string> Parameters { get; set; } = new();
        public string AccessModifier { get; set; } = "";
        public List<string> Modifiers { get; set; } = new();
        public string FullSignature { get; set; } = "";
        public int LineNumber { get; set; }
        public string ClassName { get; set; } = "";
        public List<string> Attributes { get; set; } = new();
    }

    private class PropertySignature
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string AccessModifier { get; set; } = "";
        public List<string> Modifiers { get; set; } = new();
        public string Accessors { get; set; } = "";
        public int LineNumber { get; set; }
        public List<string> Attributes { get; set; } = new();
    }

    private class DirectoryInfo
    {
        public string Name { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public List<FileInfo> Files { get; set; } = new();
        public List<DirectoryInfo> Subdirectories { get; set; } = new();
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
    }

    private class FileInfo
    {
        public string Name { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Extension { get; set; } = "";
        public int LineCount { get; set; }
    }

    // Implementation methods
    private static IEnumerable<string> GetFilesToSearch(string searchPath, string filePattern, IPathService pathService)
    {
        if (File.Exists(searchPath))
        {
            return new[] { searchPath };
        }

        var searchOption = SearchOption.AllDirectories;
        
        // Skip common ignore patterns
        var files = Directory.GetFiles(searchPath, filePattern, searchOption)
            .Where(f => !ShouldIgnoreFile(f))
            .OrderBy(f => f);

        return files;
    }

    private static bool ShouldIgnoreFile(string filePath)
    {
        var ignorePaths = new[] { "bin", "obj", ".git", "node_modules", "packages" };
        var ignoreExtensions = new[] { ".dll", ".exe", ".pdb", ".cache", ".tmp" };
        
        return ignorePaths.Any(ignore => filePath.Contains(Path.DirectorySeparatorChar + ignore + Path.DirectorySeparatorChar)) ||
               ignoreExtensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<List<SearchResult>> SearchFileWithContextAsync(string filePath, string searchText, int contextLines, IPathService pathService)
    {
        var results = new List<SearchResult>();
        var allLines = await File.ReadAllLinesAsync(filePath);
        
        for (int i = 0; i < allLines.Length; i++)
        {
            var line = allLines[i];
            if (line.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                var result = new SearchResult
                {
                    FilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath),
                    LineNumber = i + 1,
                    MatchLine = line.Trim(),
                    MatchedText = searchText
                };

                // Add context before
                var startContext = Math.Max(0, i - contextLines);
                for (int j = startContext; j < i; j++)
                {
                    result.ContextBefore.Add($"{j + 1:D4}: {allLines[j]}");
                }

                // Add context after
                var endContext = Math.Min(allLines.Length - 1, i + contextLines);
                for (int j = i + 1; j <= endContext; j++)
                {
                    result.ContextAfter.Add($"{j + 1:D4}: {allLines[j]}");
                }

                results.Add(result);
            }
        }

        return results;
    }

    private static string FormatSearchResults(List<SearchResult> results, string searchText)
    {
        if (!results.Any())
            return $"No matches found for '{searchText}'";

        var output = new System.Text.StringBuilder();
        output.AppendLine($"Found {results.Count} matches for '{searchText}':");
        output.AppendLine();

        foreach (var result in results)
        {
            output.AppendLine($"📁 {result.FilePath}:{result.LineNumber}");
            
            // Context before
            foreach (var line in result.ContextBefore)
            {
                output.AppendLine($"  {line}");
            }
            
            // Match line (highlighted)
            output.AppendLine($"► {result.LineNumber:D4}: {result.MatchLine}");
            
            // Context after
            foreach (var line in result.ContextAfter)
            {
                output.AppendLine($"  {line}");
            }
            
            output.AppendLine();
        }

        return output.ToString();
    }

    private static List<MethodSignature> ExtractMethodSignatures(string content, string[] lines, string? targetClassName)
    {
        var methods = new List<MethodSignature>();
        var currentClass = "";
        var inClass = false;
        var currentAttributes = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Track class context
            if (IsClassDeclaration(line))
            {
                currentClass = ExtractClassName(line);
                inClass = string.IsNullOrEmpty(targetClassName) || currentClass.Equals(targetClassName, StringComparison.OrdinalIgnoreCase);
            }

            // Collect attributes
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentAttributes.Add(line);
                continue;
            }

            // Look for method signatures
            if (inClass && IsMethodDeclaration(line))
            {
                var method = ParseMethodSignature(line, i + 1, currentClass);
                if (method != null)
                {
                    method.Attributes = new List<string>(currentAttributes);
                    methods.Add(method);
                }
            }

            // Clear attributes after processing a declaration
            if (!line.StartsWith("[") && !string.IsNullOrWhiteSpace(line))
            {
                currentAttributes.Clear();
            }
        }

        return methods;
    }

    private static List<PropertySignature> ExtractPropertySignatures(string content, string[] lines, string? targetClassName)
    {
        var properties = new List<PropertySignature>();
        var currentClass = "";
        var inClass = false;
        var currentAttributes = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Track class context
            if (IsClassDeclaration(line))
            {
                currentClass = ExtractClassName(line);
                inClass = string.IsNullOrEmpty(targetClassName) || currentClass.Equals(targetClassName, StringComparison.OrdinalIgnoreCase);
            }

            // Collect attributes
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentAttributes.Add(line);
                continue;
            }

            // Look for property signatures
            if (inClass && IsPropertyDeclaration(line))
            {
                var property = ParsePropertySignature(line, i + 1);
                if (property != null)
                {
                    property.Attributes = new List<string>(currentAttributes);
                    properties.Add(property);
                }
            }

            // Clear attributes after processing a declaration
            if (!line.StartsWith("[") && !string.IsNullOrWhiteSpace(line))
            {
                currentAttributes.Clear();
            }
        }

        return properties;
    }

    private static bool IsClassDeclaration(string line)
    {
        return Regex.IsMatch(line, @"^\s*(public|private|protected|internal)?\s*(static|abstract|sealed)?\s*class\s+\w+", RegexOptions.IgnoreCase);
    }

    private static string ExtractClassName(string line)
    {
        var match = Regex.Match(line, @"class\s+(\w+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : "";
    }

    private static bool IsMethodDeclaration(string line)
    {
        // Skip properties, events, fields
        if (line.Contains(" { get") || line.Contains(" { set") || line.Contains("event ") || line.EndsWith(";"))
            return false;

        // Look for method pattern: access_modifier [modifiers] return_type method_name(parameters)
        return Regex.IsMatch(line, @"^\s*(public|private|protected|internal)\s+.*\w+\s*\([^)]*\)\s*(\{|$)", RegexOptions.IgnoreCase);
    }

    private static bool IsPropertyDeclaration(string line)
    {
        return Regex.IsMatch(line, @"^\s*(public|private|protected|internal)\s+.*\w+\s*\{\s*(get|set)", RegexOptions.IgnoreCase);
    }

    private static MethodSignature? ParseMethodSignature(string line, int lineNumber, string className)
    {
        try
        {
            // Simplified parsing - this is a basic implementation
            var method = new MethodSignature
            {
                LineNumber = lineNumber,
                ClassName = className,
                FullSignature = line.Trim()
            };

            // Basic extraction - would need more sophisticated parsing for production
            if (line.Contains("public")) method.AccessModifier = "public";
            else if (line.Contains("private")) method.AccessModifier = "private";
            else if (line.Contains("protected")) method.AccessModifier = "protected";
            else if (line.Contains("internal")) method.AccessModifier = "internal";

            // Extract method name (basic)
            var parenIndex = line.IndexOf('(');
            if (parenIndex > 0)
            {
                var beforeParen = line.Substring(0, parenIndex).Trim();
                var parts = beforeParen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    method.Name = parts[^1]; // Last part before parentheses
                }
            }

            return method;
        }
        catch
        {
            return null;
        }
    }

    private static PropertySignature? ParsePropertySignature(string line, int lineNumber)
    {
        try
        {
            var property = new PropertySignature
            {
                LineNumber = lineNumber
            };

            // Basic extraction - would need more sophisticated parsing for production
            if (line.Contains("public")) property.AccessModifier = "public";
            else if (line.Contains("private")) property.AccessModifier = "private";
            else if (line.Contains("protected")) property.AccessModifier = "protected";
            else if (line.Contains("internal")) property.AccessModifier = "internal";

            // Extract property name (basic)
            var braceIndex = line.IndexOf('{');
            if (braceIndex > 0)
            {
                var beforeBrace = line.Substring(0, braceIndex).Trim();
                var parts = beforeBrace.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    property.Type = parts[^2];
                    property.Name = parts[^1];
                }
            }

            // Extract accessors
            var braceContent = line.Substring(line.IndexOf('{'));
            property.Accessors = braceContent;

            return property;
        }
        catch
        {
            return null;
        }
    }

    private static string FormatSignatureOutput(string path, List<MethodSignature> methods, List<PropertySignature> properties, string? className)
    {
        var output = new System.Text.StringBuilder();
        
        var title = string.IsNullOrEmpty(className) 
            ? $"Method signatures from {path}" 
            : $"Method signatures for class '{className}' in {path}";
            
        output.AppendLine(title);
        output.AppendLine(new string('=', title.Length));
        output.AppendLine();

        if (properties.Any())
        {
            output.AppendLine("PROPERTIES:");
            output.AppendLine(new string('-', 10));
            foreach (var prop in properties.OrderBy(p => p.LineNumber))
            {
                if (prop.Attributes.Any())
                {
                    foreach (var attr in prop.Attributes)
                    {
                        output.AppendLine($"    {attr}");
                    }
                }
                
                output.AppendLine($"    {prop.AccessModifier} {prop.Type} {prop.Name} {prop.Accessors}");
                output.AppendLine();
            }
        }

        if (methods.Any())
        {
            output.AppendLine("METHODS:");
            output.AppendLine(new string('-', 8));
            foreach (var method in methods.OrderBy(m => m.LineNumber))
            {
                if (method.Attributes.Any())
                {
                    foreach (var attr in method.Attributes)
                    {
                        output.AppendLine($"    {attr}");
                    }
                }
                
                output.AppendLine($"    {method.AccessModifier} {method.Name}() // Line {method.LineNumber}");
                output.AppendLine();
            }
        }

        if (!methods.Any() && !properties.Any())
        {
            output.AppendLine("No method or property signatures found.");
        }

        return output.ToString();
    }

    private static HashSet<string> ParseFileTypes(string fileTypes)
    {
        if (string.IsNullOrEmpty(fileTypes))
            return new HashSet<string>();

        return fileTypes.Split(',', ';')
            .Select(ext => ext.Trim().ToLowerInvariant())
            .Select(ext => ext.StartsWith(".") ? ext : "." + ext)
            .ToHashSet();
    }

    private static async Task<DirectoryInfo> AnalyzeDirectoryAsync(string fullPath, string relativePath, int currentDepth, 
        int maxDepth, HashSet<string> allowedExtensions, bool includeHidden, bool includeDetails, IPathService pathService)
    {
        var dirInfo = new DirectoryInfo
        {
            Name = Path.GetFileName(fullPath) ?? "root",
            RelativePath = relativePath
        };

        try
        {
            // Process files
            var files = Directory.GetFiles(fullPath)
                .Where(f => ShouldIncludeFile(f, allowedExtensions, includeHidden))
                .ToList();

            foreach (var file in files)
            {
                var fileInfo = await CreateFileInfoAsync(file, fullPath, includeDetails);
                dirInfo.Files.Add(fileInfo);
                dirInfo.TotalSize += fileInfo.Size;
            }

            dirInfo.TotalFiles = dirInfo.Files.Count;

            // Process subdirectories
            if (currentDepth < maxDepth)
            {
                var directories = Directory.GetDirectories(fullPath)
                    .Where(d => ShouldIncludeDirectory(d, includeHidden))
                    .ToList();

                foreach (var directory in directories)
                {
                    var subRelativePath = Path.Combine(relativePath, Path.GetFileName(directory));
                    var subDirInfo = await AnalyzeDirectoryAsync(directory, subRelativePath, currentDepth + 1, 
                        maxDepth, allowedExtensions, includeHidden, includeDetails, pathService);
                    
                    dirInfo.Subdirectories.Add(subDirInfo);
                    dirInfo.TotalFiles += subDirInfo.TotalFiles;
                    dirInfo.TotalSize += subDirInfo.TotalSize;
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we can't access
        }

        return dirInfo;
    }

    private static async Task<FileInfo> CreateFileInfoAsync(string filePath, string basePath, bool includeDetails)
    {
        var file = new System.IO.FileInfo(filePath);
        var fileInfo = new FileInfo
        {
            Name = file.Name,
            RelativePath = Path.GetRelativePath(basePath, filePath),
            Extension = file.Extension.ToLowerInvariant(),
            LastModified = file.LastWriteTime
        };

        if (includeDetails)
        {
            fileInfo.Size = file.Length;
            
            // Count lines for text files
            if (IsTextFile(fileInfo.Extension))
            {
                try
                {
                    var lines = await File.ReadAllLinesAsync(filePath);
                    fileInfo.LineCount = lines.Length;
                }
                catch
                {
                    fileInfo.LineCount = 0;
                }
            }
        }

        return fileInfo;
    }

    private static bool ShouldIncludeFile(string filePath, HashSet<string> allowedExtensions, bool includeHidden)
    {
        var fileName = Path.GetFileName(filePath);
        
        // Check hidden files
        if (!includeHidden && fileName.StartsWith("."))
            return false;

        // Check file extension filter
        if (allowedExtensions.Any())
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

        // Skip certain file types by default
        var skipExtensions = new[] { ".dll", ".exe", ".pdb", ".cache", ".tmp", ".log" };
        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return !skipExtensions.Contains(fileExtension);
    }

    private static bool ShouldIncludeDirectory(string dirPath, bool includeHidden)
    {
        var dirName = Path.GetFileName(dirPath);
        
        // Check hidden directories
        if (!includeHidden && dirName.StartsWith("."))
            return false;

        // Skip common ignore directories
        var ignoreDirs = new[] { "bin", "obj", "node_modules", "packages", ".git", ".vs", ".vscode" };
        return !ignoreDirs.Contains(dirName.ToLowerInvariant());
    }

    private static bool IsTextFile(string extension)
    {
        var textExtensions = new[] { ".cs", ".js", ".ts", ".json", ".xml", ".txt", ".md", ".yml", ".yaml", 
            ".css", ".html", ".htm", ".sql", ".config", ".csproj", ".sln", ".gitignore" };
        return textExtensions.Contains(extension);
    }

    private static string FormatDirectoryTree(DirectoryInfo dirInfo, bool includeDetails, string sortBy)
    {
        var output = new System.Text.StringBuilder();
        
        output.AppendLine($"📁 Directory Tree Summary: {dirInfo.Name}");
        output.AppendLine(new string('=', 50));
        
        if (includeDetails)
        {
            output.AppendLine($"Total Files: {dirInfo.TotalFiles}");
            output.AppendLine($"Total Size: {FormatFileSize(dirInfo.TotalSize)}");
            output.AppendLine();
        }

        FormatDirectoryRecursive(output, dirInfo, "", includeDetails, sortBy);
        
        return output.ToString();
    }

    private static void FormatDirectoryRecursive(System.Text.StringBuilder output, DirectoryInfo dirInfo, string indent, 
        bool includeDetails, string sortBy)
    {
        // Sort files
        var sortedFiles = SortFiles(dirInfo.Files, sortBy);
        
        // Display files
        foreach (var file in sortedFiles)
        {
            var icon = GetFileIcon(file.Extension);
            var details = includeDetails ? $" ({FormatFileSize(file.Size)}" + 
                (file.LineCount > 0 ? $", {file.LineCount} lines" : "") + ")" : "";
            
            output.AppendLine($"{indent}{icon} {file.Name}{details}");
        }

        // Display subdirectories
        foreach (var subDir in dirInfo.Subdirectories.OrderBy(d => d.Name))
        {
            var details = includeDetails ? $" ({subDir.TotalFiles} files, {FormatFileSize(subDir.TotalSize)})" : "";
            output.AppendLine($"{indent}📁 {subDir.Name}/{details}");
            
            if (subDir.Files.Any() || subDir.Subdirectories.Any())
            {
                FormatDirectoryRecursive(output, subDir, indent + "  ", includeDetails, sortBy);
            }
        }
    }

    private static List<FileInfo> SortFiles(List<FileInfo> files, string sortBy)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "size" => files.OrderByDescending(f => f.Size).ToList(),
            "modified" => files.OrderByDescending(f => f.LastModified).ToList(),
            "extension" => files.OrderBy(f => f.Extension).ThenBy(f => f.Name).ToList(),
            _ => files.OrderBy(f => f.Name).ToList()
        };
    }

    private static string GetFileIcon(string extension)
    {
        return extension switch
        {
            ".cs" => "🔷",
            ".js" => "📜",
            ".ts" => "📘",
            ".json" => "📋",
            ".xml" => "📄",
            ".md" => "📝",
            ".txt" => "📄",
            ".sql" => "🗃️",
            ".config" => "⚙️",
            ".csproj" => "🏗️",
            ".sln" => "🏗️",
            _ => "📄"
        };
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }
}
