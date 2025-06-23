using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO.Abstractions;
using System.Text;

namespace CodeEditor.MCP.Services;
public class CSharpFormattingService : ICSharpFormattingService
{
    private readonly IFileService _fileService;
    private readonly IPathService _pathService;
    private readonly IFileSystem _fileSystem;
    public CSharpFormattingService(IFileService fileService, IPathService pathService, IFileSystem fileSystem)
    {
        _fileService = fileService;
        _pathService = pathService;
        _fileSystem = fileSystem;
    }

    public string FormatDocument(string relativePath)
    {
        try
        {
            var fullPath = _pathService.GetFullPath(relativePath);
            if (!_fileSystem.File.Exists(fullPath))
            {
                return $"Error: File not found at path: {relativePath}";
            }

            if (!fullPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                return $"Error: File must be a C# source file (.cs): {relativePath}";
            }

            var sourceText = _fileSystem.File.ReadAllText(fullPath);
            // Parse the source code
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
            var root = syntaxTree.GetRoot();
            // Check for syntax errors
            var diagnostics = syntaxTree.GetDiagnostics();
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Any())
            {
                var errorMessages = errors.Select(e => $"Line {e.Location.GetLineSpan().StartLinePosition.Line + 1}: {e.GetMessage()}");
                return $"Error: Cannot format document due to syntax errors:\n{string.Join("\n", errorMessages)}";
            }

            // Use NormalizeWhitespace for reliable formatting without workspace dependencies
            var normalizedRoot = root.NormalizeWhitespace();
            var formattedText = normalizedRoot.ToFullString();
            // Write the formatted content back to the file
            _fileSystem.File.WriteAllText(fullPath, formattedText, Encoding.UTF8);
            return $"Successfully formatted: {relativePath}";
        }
        catch (Exception ex)
        {
            return $"Error formatting document {relativePath}: {ex.Message}";
        }
    }

    public string FormatDirectory(string relativePath, bool recursive = false)
    {
        try
        {
            var fullPath = _pathService.GetFullPath(relativePath);
            if (!_fileSystem.Directory.Exists(fullPath))
            {
                return $"Error: Directory not found at path: {relativePath}";
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var csFiles = _fileSystem.Directory.GetFiles(fullPath, "*.cs", searchOption);
            if (!csFiles.Any())
            {
                return $"No C# files found in directory: {relativePath}";
            }

            var results = new List<string>();
            var successCount = 0;
            var errorCount = 0;
            foreach (var file in csFiles)
            {
                var relativeFilePath = _pathService.GetRelativePath(file);
                var result = FormatDocument(relativeFilePath);
                if (result.StartsWith("Successfully"))
                {
                    successCount++;
                    results.Add($"✓ {relativeFilePath}");
                }
                else
                {
                    errorCount++;
                    results.Add($"✗ {relativeFilePath}: {result}");
                }
            }

            var summary = new StringBuilder();
            summary.AppendLine($"Formatting complete for directory: {relativePath}");
            summary.AppendLine($"Files processed: {csFiles.Length}");
            summary.AppendLine($"Successfully formatted: {successCount}");
            summary.AppendLine($"Errors: {errorCount}");
            if (results.Any())
            {
                summary.AppendLine("\nDetailed results:");
                summary.AppendLine(string.Join("\n", results));
            }

            return summary.ToString();
        }
        catch (Exception ex)
        {
            return $"Error formatting directory {relativePath}: {ex.Message}";
        }
    }
}