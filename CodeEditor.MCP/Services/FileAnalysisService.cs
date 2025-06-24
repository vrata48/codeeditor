using System.Text;
using System.Text.RegularExpressions;
using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services;

public class FileAnalysisService : IFileAnalysisService
{
    private readonly IPathService _pathService;
    private readonly IFileFilterService _fileFilterService;

    public FileAnalysisService(IPathService pathService, IFileFilterService fileFilterService)
    {
        _pathService = pathService;
        _fileFilterService = fileFilterService;
    }

    public async Task<string> ReadFileLinesAsync(string path, int startLine, int endLine)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (startLine < 1)
            throw new ArgumentException("Start line must be greater than 0", nameof(startLine));

        if (endLine < startLine)
            throw new ArgumentException("End line must be greater than or equal to start line", nameof(endLine));

        var fullPath = _pathService.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {fullPath}");

        var lines = await File.ReadAllLinesAsync(fullPath);
        
        if (startLine > lines.Length)
            throw new ArgumentException($"Start line {startLine} exceeds file length {lines.Length}", nameof(startLine));

        var adjustedEndLine = Math.Min(endLine, lines.Length);
        var selectedLines = lines.Skip(startLine - 1).Take(adjustedEndLine - startLine + 1);
        
        var result = new StringBuilder();
        var currentLine = startLine;
        
        foreach (var line in selectedLines)
        {
            result.AppendLine($"{currentLine:D4}: {line}");
            currentLine++;
        }

        return result.ToString();
    }

    public async Task<string> ReadAroundLineAsync(string path, int centerLine, int contextLines = 5)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (centerLine < 1)
            throw new ArgumentException("Center line must be greater than 0", nameof(centerLine));

        if (contextLines < 0)
            throw new ArgumentException("Context lines cannot be negative", nameof(contextLines));

        var startLine = Math.Max(1, centerLine - contextLines);
        var endLine = centerLine + contextLines;

        return await ReadFileLinesAsync(path, startLine, endLine);
    }

    public async Task<string> SearchFilesWithContextAsync(string text, string path = ".", int contextLines = 3, string filePattern = "*", int maxResults = 20)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Search text cannot be null or empty", nameof(text));

        var searchPath = string.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : _pathService.GetFullPath(path);

        if (!Directory.Exists(searchPath) && !File.Exists(searchPath))
            throw new DirectoryNotFoundException($"Path not found: {searchPath}");

        var results = new List<SearchResult>();
        var files = GetFilesToSearch(searchPath, filePattern);

        foreach (var file in files.Take(100)) // Limit files to search
        {
            try
            {
                var matches = await SearchFileWithContextAsync(file, text, contextLines);
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

    public async Task<string> GetMethodSignaturesAsync(string path, string? className = null, bool includeProperties = true)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        var fullPath = _pathService.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {fullPath}");

        var content = await File.ReadAllTextAsync(fullPath);
        var lines = content.Split('\n');

        var methods = ExtractMethodSignatures(lines, className);
        var properties = includeProperties ? ExtractPropertySignatures(lines, className) : new List<PropertySignature>();

        return FormatSignatureOutput(methods, properties, className);
    }

    public async Task<string> GetFileTreeSummaryAsync(string path = ".", int maxDepth = 3, string fileTypes = "", bool includeHidden = false, bool includeDetails = true, string sortBy = "name")
    {
        var searchPath = string.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : _pathService.GetFullPath(path);

        if (!Directory.Exists(searchPath))
            throw new DirectoryNotFoundException($"Directory not found: {searchPath}");

        var allowedExtensions = ParseFileTypes(fileTypes);
        var dirInfo = await AnalyzeDirectoryAsync(searchPath, "", 0, maxDepth, allowedExtensions, includeHidden, includeDetails);

        return FormatDirectoryTree(dirInfo, includeDetails, sortBy);
    }

    private IEnumerable<string> GetFilesToSearch(string searchPath, string filePattern)
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

    private bool ShouldIgnoreFile(string filePath)
    {
        var relativePath = _pathService.GetRelativePath(filePath);
        return !_fileFilterService.ShouldInclude(relativePath);
    }

    private async Task<List<SearchResult>> SearchFileWithContextAsync(string filePath, string searchText, int contextLines)
    {
        var results = new List<SearchResult>();
        var lines = await File.ReadAllLinesAsync(filePath);
        var relativePath = _pathService.GetRelativePath(filePath);

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                var startLine = Math.Max(0, i - contextLines);
                var endLine = Math.Min(lines.Length - 1, i + contextLines);
                
                var contextBefore = new List<string>();
                var contextAfter = new List<string>();
                
                for (int j = startLine; j < i; j++)
                {
                    contextBefore.Add($"{j + 1:D4}: {lines[j]}");
                }
                
                for (int j = i + 1; j <= endLine; j++)
                {
                    contextAfter.Add($"{j + 1:D4}: {lines[j]}");
                }

                results.Add(new SearchResult
                {
                    FilePath = relativePath,
                    LineNumber = i + 1,
                    MatchLine = lines[i],
                    ContextBefore = contextBefore,
                    ContextAfter = contextAfter,
                    MatchedText = searchText
                });
            }
        }

        return results;
    }

    private string FormatSearchResults(List<SearchResult> results, string searchText)
    {
        if (!results.Any())
            return $"No matches found for '{searchText}'";

        var output = new StringBuilder();
        output.AppendLine($"Found {results.Count} matches for '{searchText}':");
        output.AppendLine();

        var groupedResults = results.GroupBy(r => r.FilePath);

        foreach (var fileGroup in groupedResults)
        {
            output.AppendLine($"📁 {fileGroup.Key}");
            
            foreach (var result in fileGroup)
            {
                output.AppendLine($"   Line {result.LineNumber}:");
                
                foreach (var contextLine in result.ContextBefore)
                {
                    output.AppendLine($"   {contextLine}");
                }
                
                output.AppendLine($">>> {result.LineNumber:D4}: {result.MatchLine}");
                
                foreach (var contextLine in result.ContextAfter)
                {
                    output.AppendLine($"   {contextLine}");
                }
                
                output.AppendLine();
            }
        }

        return output.ToString();
    }

    private List<MethodSignature> ExtractMethodSignatures(string[] lines, string? targetClassName)
    {
        var methods = new List<MethodSignature>();
        string? currentClass = null;
        bool inClass = false;
        int braceCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Track class declarations
            if (IsClassDeclaration(line))
            {
                currentClass = ExtractClassName(line);
                inClass = targetClassName == null || currentClass == targetClassName;
                braceCount = 0;
                continue;
            }

            // Track braces to know when we're inside/outside classes
            braceCount += line.Count(c => c == '{') - line.Count(c => c == '}');
            
            if (braceCount <= 0)
            {
                inClass = false;
                currentClass = null;
            }

            // Only process methods if we're in the target class (or any class if no target specified)
            if (inClass && IsMethodDeclaration(line) && !IsPropertyDeclaration(line))
            {
                var signature = ParseMethodSignature(line, currentClass, i + 1);
                if (signature != null)
                {
                    methods.Add(signature);
                }
            }
        }

        return methods;
    }

    private List<PropertySignature> ExtractPropertySignatures(string[] lines, string? targetClassName)
    {
        var properties = new List<PropertySignature>();
        string? currentClass = null;
        bool inClass = false;
        int braceCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Track class declarations
            if (IsClassDeclaration(line))
            {
                currentClass = ExtractClassName(line);
                inClass = targetClassName == null || currentClass == targetClassName;
                braceCount = 0;
                continue;
            }

            // Track braces
            braceCount += line.Count(c => c == '{') - line.Count(c => c == '}');
            
            if (braceCount <= 0)
            {
                inClass = false;
                currentClass = null;
            }

            // Only process properties if we're in the target class
            if (inClass && IsPropertyDeclaration(line))
            {
                var signature = ParsePropertySignature(line, i + 1);
                if (signature != null)
                {
                    properties.Add(signature);
                }
            }
        }

        return properties;
    }

    private bool IsClassDeclaration(string line)
    {
        return Regex.IsMatch(line, @"^\s*(public|private|protected|internal)?\s*class\s+\w+");
    }

    private string? ExtractClassName(string line)
    {
        var match = Regex.Match(line, @"class\s+(\w+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private bool IsMethodDeclaration(string line)
    {
        // Method pattern: access modifier + return type + method name + parameters
        return Regex.IsMatch(line, @"^\s*(public|private|protected|internal)?\s*(static|virtual|override|abstract)?\s*\w+\s+\w+\s*\([^)]*\)\s*(where\s+.*?)?\s*[{;]");
    }

    private bool IsPropertyDeclaration(string line)
    {
        // Property pattern: access modifier + type + property name + { get/set
        return Regex.IsMatch(line, @"^\s*(public|private|protected|internal)?\s*(static|virtual|override|abstract)?\s*\w+\s+\w+\s*{\s*(get|set)");
    }

    private MethodSignature? ParseMethodSignature(string line, string? className, int lineNumber)
    {
        try
        {
            // Remove leading/trailing whitespace and normalize
            line = line.Trim();
            
            // Extract access modifier
            var accessMatch = Regex.Match(line, @"^\s*(public|private|protected|internal)");
            var accessModifier = accessMatch.Success ? accessMatch.Groups[1].Value : "private";

            // Extract modifiers (static, virtual, etc.)
            var modifierMatches = Regex.Matches(line, @"\b(static|virtual|override|abstract|async)\b");
            var modifiers = modifierMatches.Cast<Match>().Select(m => m.Value).ToList();

            // Extract return type and method name
            var methodMatch = Regex.Match(line, @"(\w+)\s+(\w+)\s*\(([^)]*)\)");
            if (!methodMatch.Success) return null;

            var returnType = methodMatch.Groups[1].Value;
            var methodName = methodMatch.Groups[2].Value;
            var parametersStr = methodMatch.Groups[3].Value.Trim();
            
            var parameters = string.IsNullOrEmpty(parametersStr) 
                ? new List<string>() 
                : parametersStr.Split(',').Select(p => p.Trim()).ToList();

            return new MethodSignature
            {
                Name = methodName,
                ReturnType = returnType,
                AccessModifier = accessModifier,
                Modifiers = modifiers,
                Parameters = parameters,
                FullSignature = line,
                LineNumber = lineNumber,
                ClassName = className ?? "",
                Attributes = new List<string>()
            };
        }
        catch
        {
            return null;
        }
    }

    private PropertySignature? ParsePropertySignature(string line, int lineNumber)
    {
        try
        {
            line = line.Trim();
            
            // Extract access modifier
            var accessMatch = Regex.Match(line, @"^\s*(public|private|protected|internal)");
            var accessModifier = accessMatch.Success ? accessMatch.Groups[1].Value : "private";

            // Extract modifiers
            var modifierMatches = Regex.Matches(line, @"\b(static|virtual|override|abstract)\b");
            var modifiers = modifierMatches.Cast<Match>().Select(m => m.Value).ToList();

            // Extract property type and name
            var propertyMatch = Regex.Match(line, @"(\w+)\s+(\w+)\s*{");
            if (!propertyMatch.Success) return null;

            var propertyType = propertyMatch.Groups[1].Value;
            var propertyName = propertyMatch.Groups[2].Value;

            // Extract accessors
            var accessors = "";
            if (line.Contains("get") && line.Contains("set"))
                accessors = "get; set;";
            else if (line.Contains("get"))
                accessors = "get;";
            else if (line.Contains("set"))
                accessors = "set;";

            return new PropertySignature
            {
                Name = propertyName,
                Type = propertyType,
                AccessModifier = accessModifier,
                Modifiers = modifiers,
                Accessors = accessors,
                LineNumber = lineNumber,
                Attributes = new List<string>()
            };
        }
        catch
        {
            return null;
        }
    }

    private string FormatSignatureOutput(List<MethodSignature> methods, List<PropertySignature> properties, string? className)
    {
        var output = new StringBuilder();
        
        var title = className != null 
            ? $"Method signatures for class '{className}'" 
            : "Method signatures";
            
        output.AppendLine($"{title} in {methods.FirstOrDefault()?.ClassName ?? "file"}");
        output.AppendLine(new string('=', title.Length + 20));
        output.AppendLine();

        if (properties.Any())
        {
            output.AppendLine("PROPERTIES:");
            output.AppendLine("----------");
            foreach (var prop in properties.OrderBy(p => p.Name))
            {
                var modifierStr = prop.Modifiers.Any() ? $"{string.Join(" ", prop.Modifiers)} " : "";
                output.AppendLine($"    {prop.AccessModifier} {modifierStr}{prop.Type} {prop.Name} {{ {prop.Accessors} }}");
            }
            output.AppendLine();
        }

        if (methods.Any())
        {
            output.AppendLine("METHODS:");
            output.AppendLine("--------");
            foreach (var method in methods.OrderBy(m => m.Name))
            {
                var modifierStr = method.Modifiers.Any() ? $"{string.Join(" ", method.Modifiers)} " : "";
                var paramStr = method.Parameters.Any() ? string.Join(", ", method.Parameters) : "";
                output.AppendLine($"    {method.AccessModifier} {modifierStr}{method.ReturnType} {method.Name}({paramStr})");
            }
        }

        if (!methods.Any() && !properties.Any())
        {
            output.AppendLine("No method or property signatures found.");
        }

        return output.ToString();
    }

    private HashSet<string> ParseFileTypes(string fileTypes)
    {
        if (string.IsNullOrEmpty(fileTypes))
            return new HashSet<string>();

        return fileTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim().ToLowerInvariant())
            .Select(ext => ext.StartsWith('.') ? ext : $".{ext}")
            .ToHashSet();
    }

    private async Task<CodeEditor.MCP.Models.DirectoryInfo> AnalyzeDirectoryAsync(
        string directoryPath, 
        string relativePath, 
        int currentDepth, 
        int maxDepth, 
        HashSet<string> allowedExtensions, 
        bool includeHidden, 
        bool includeDetails)
    {
        var dirInfo = new CodeEditor.MCP.Models.DirectoryInfo
        {
            Name = string.IsNullOrEmpty(relativePath) ? Path.GetFileName(directoryPath) : Path.GetFileName(relativePath),
            RelativePath = relativePath,
            Files = new List<CodeEditor.MCP.Models.FileInfo>(),
            Subdirectories = new List<CodeEditor.MCP.Models.DirectoryInfo>()
        };

        try
        {
            // Add files
            foreach (var filePath in Directory.GetFiles(directoryPath))
            {
                var fileName = Path.GetFileName(filePath);
                var fileRelativePath = string.IsNullOrEmpty(relativePath) 
                    ? fileName 
                    : $"{relativePath}/{fileName}";

                if (ShouldIncludeFile(filePath, allowedExtensions, includeHidden))
                {
                    var fileInfo = await CreateFileInfoAsync(filePath, fileRelativePath, includeDetails);
                    dirInfo.Files.Add(fileInfo);
                }
            }

            // Add subdirectories (if not at max depth)
            if (currentDepth < maxDepth)
            {
                foreach (var subDirPath in Directory.GetDirectories(directoryPath))
                {
                    var subDirName = Path.GetFileName(subDirPath);
                    var subDirRelativePath = string.IsNullOrEmpty(relativePath) 
                        ? subDirName 
                        : $"{relativePath}/{subDirName}";

                    if (ShouldIncludeDirectoryForTreeSummary(subDirPath, includeHidden))
                    {
                        var subDirInfo = await AnalyzeDirectoryAsync(
                            subDirPath, 
                            subDirRelativePath, 
                            currentDepth + 1, 
                            maxDepth, 
                            allowedExtensions, 
                            includeHidden, 
                            includeDetails);
                        
                        dirInfo.Subdirectories.Add(subDirInfo);
                    }
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we don't have access to
        }

        return dirInfo;
    }

    private bool ShouldIncludeFile(string filePath, HashSet<string> allowedExtensions, bool includeHidden)
    {
        var fileName = Path.GetFileName(filePath);
        
        // Check hidden files
        if (!includeHidden && fileName.StartsWith("."))
            return false;

        // Check file extension filter (from fileTypes parameter)
        if (allowedExtensions.Any())
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return false;
        }

        // Check global filter and gitignore
        var relativePath = _pathService.GetRelativePath(filePath);
        return _fileFilterService.ShouldInclude(relativePath);
    }

    private string FormatDirectoryTree(CodeEditor.MCP.Models.DirectoryInfo dirInfo, bool includeDetails, string sortBy)
    {
        var output = new StringBuilder();
        
        // Calculate totals
        var totalFiles = CountTotalFiles(dirInfo);
        var totalSize = CalculateTotalSize(dirInfo);
        
        output.AppendLine($"📁 Directory Tree Summary: {dirInfo.Name}");
        output.AppendLine("==================================================");
        output.AppendLine($"Total Files: {totalFiles}");
        if (includeDetails)
        {
            output.AppendLine($"Total Size: {FormatFileSize(totalSize)}");
        }
        output.AppendLine();
        
        FormatDirectoryRecursive(dirInfo, output, "", true, includeDetails, sortBy);
        
        return output.ToString();
    }

    private void FormatDirectoryRecursive(
        CodeEditor.MCP.Models.DirectoryInfo dirInfo, 
        StringBuilder output, 
        string prefix, 
        bool isLast, 
        bool includeDetails, 
        string sortBy)
    {
        // Directory header
        var dirPrefix = isLast ? "📁 " : "📁 ";
        var dirSuffix = "";
        
        if (includeDetails)
        {
            var fileCount = CountTotalFiles(dirInfo);
            var totalSize = CalculateTotalSize(dirInfo);
            dirSuffix = $" ({fileCount} files, {FormatFileSize(totalSize)})";
        }
        
        output.AppendLine($"{prefix}{dirPrefix}{dirInfo.Name}/{dirSuffix}");
        
        var newPrefix = prefix + (isLast ? "  " : "│ ");
        
        // Sort and display files
        var sortedFiles = SortFiles(dirInfo.Files, sortBy);
        for (int i = 0; i < sortedFiles.Count; i++)
        {
            var file = sortedFiles[i];
            var isLastFile = i == sortedFiles.Count - 1 && !dirInfo.Subdirectories.Any();
            var fileIcon = GetFileIcon(file.Name);
            var fileSuffix = includeDetails ? $" ({FormatFileSize(file.Size)}, {file.LineCount} lines)" : "";
            
            output.AppendLine($"{newPrefix}{fileIcon} {file.Name}{fileSuffix}");
        }
        
        // Sort and display subdirectories
        var sortedDirs = dirInfo.Subdirectories.OrderBy(d => d.Name).ToList();
        for (int i = 0; i < sortedDirs.Count; i++)
        {
            var subDir = sortedDirs[i];
            var isLastSubDir = i == sortedDirs.Count - 1;
            
            FormatDirectoryRecursive(subDir, output, newPrefix, isLastSubDir, includeDetails, sortBy);
        }
    }

    private List<CodeEditor.MCP.Models.FileInfo> SortFiles(List<CodeEditor.MCP.Models.FileInfo> files, string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "size" => files.OrderByDescending(f => f.Size).ToList(),
            "modified" => files.OrderByDescending(f => f.LastModified).ToList(),
            "extension" => files.OrderBy(f => Path.GetExtension(f.Name)).ThenBy(f => f.Name).ToList(),
            _ => files.OrderBy(f => f.Name).ToList()
        };
    }

    private string GetFileIcon(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".cs" => "🔷",
            ".js" => "🟨",
            ".ts" => "🔵",
            ".html" => "🌐",
            ".css" => "🎨",
            ".json" => "📋",
            ".xml" => "📋",
            ".md" => "📝",
            ".txt" => "📄",
            ".yml" or ".yaml" => "⚙️",
            ".png" or ".jpg" or ".jpeg" or ".gif" => "🖼️",
            _ => "📄"
        };
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private int CountTotalFiles(CodeEditor.MCP.Models.DirectoryInfo dirInfo)
    {
        return dirInfo.Files.Count + dirInfo.Subdirectories.Sum(CountTotalFiles);
    }

    private long CalculateTotalSize(CodeEditor.MCP.Models.DirectoryInfo dirInfo)
    {
        return dirInfo.Files.Sum(f => f.Size) + dirInfo.Subdirectories.Sum(CalculateTotalSize);
    }

    private bool ShouldIncludeDirectoryForTreeSummary(string directoryPath, bool includeHidden)
    {
        var dirName = Path.GetFileName(directoryPath);
        
        if (!includeHidden && dirName.StartsWith("."))
            return false;

        // Check if directory should be ignored by gitignore
        var relativePath = _pathService.GetRelativePath(directoryPath);
        return !_pathService.ShouldIgnoreDirectory(relativePath);
    }

    private async Task<CodeEditor.MCP.Models.FileInfo> CreateFileInfoAsync(string filePath, string relativePath, bool includeDetails)
    {
        var fileInfo = new System.IO.FileInfo(filePath);
        var result = new CodeEditor.MCP.Models.FileInfo
        {
            Name = fileInfo.Name,
            RelativePath = relativePath,
            Size = fileInfo.Length,
            LastModified = fileInfo.LastWriteTime
        };

        if (includeDetails)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                result.LineCount = lines.Length;
            }
            catch
            {
                result.LineCount = 0;
            }
        }

        return result;
    }
}
