using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using System.IO.Abstractions;
using System.Text;

namespace CodeEditor.MCP.Services;

public class DocumentFormattingService : IDocumentFormattingService
{
    private readonly IFileService _fileService;
    private readonly IPathService _pathService;
    private readonly IFileSystem _fileSystem;

    public DocumentFormattingService(IFileService fileService, IPathService pathService, IFileSystem fileSystem)
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

            // Create workspace and apply formatting
            using var workspace = new AdhocWorkspace();
            var formattedRoot = Formatter.Format(root, workspace);
            
            var formattedText = formattedRoot.ToFullString();
            
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

    public string ValidateFormatting(string relativePath)
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

            // Check for syntax errors first
            var diagnostics = syntaxTree.GetDiagnostics();
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            
            if (errors.Any())
            {
                var errorMessages = errors.Select(e => $"Line {e.Location.GetLineSpan().StartLinePosition.Line + 1}: {e.GetMessage()}");
                return $"Validation failed - Syntax errors found:\n{string.Join("\n", errorMessages)}";
            }

            // Format the document and compare
            using var workspace = new AdhocWorkspace();
            var formattedRoot = Formatter.Format(root, workspace);
            var formattedText = formattedRoot.ToFullString();

            var isFormatted = string.Equals(sourceText.Trim(), formattedText.Trim(), StringComparison.Ordinal);

            if (isFormatted)
            {
                return $"✓ Document is properly formatted: {relativePath}";
            }
            else
            {
                // Count differences for a summary
                var originalLines = sourceText.Split('\n');
                var formattedLines = formattedText.Split('\n');
                var maxLines = Math.Max(originalLines.Length, formattedLines.Length);
                var differenceCount = 0;

                for (int i = 0; i < maxLines; i++)
                {
                    var originalLine = i < originalLines.Length ? originalLines[i].TrimEnd() : "";
                    var formattedLine = i < formattedLines.Length ? formattedLines[i].TrimEnd() : "";
                    
                    if (originalLine != formattedLine)
                    {
                        differenceCount++;
                    }
                }

                return $"✗ Document formatting issues found: {relativePath}\n" +
                       $"Lines with formatting differences: {differenceCount}\n" +
                       $"Run FormatDocument to fix formatting issues.";
            }
        }
        catch (Exception ex)
        {
            return $"Error validating formatting for {relativePath}: {ex.Message}";
        }
    }
}
