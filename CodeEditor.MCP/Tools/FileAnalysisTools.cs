using System.ComponentModel;
using CodeEditor.MCP.Services;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
public static class FileAnalysisTools
{
    [McpServerTool]
    [Description("Read specific line ranges from files")]
    public static async Task<string> ReadFileLines(
        IFileAnalysisService fileAnalysisService,
        [Description("Relative path to file")] string path,
        [Description("Starting line number (1-based)")] int startLine,
        [Description("Ending line number (1-based, inclusive)")] int endLine)
    {
        return await fileAnalysisService.ReadFileLinesAsync(path, startLine, endLine);
    }

    [McpServerTool]
    [Description("Read lines around a specific line number")]
    public static async Task<string> ReadAroundLine(
        IFileAnalysisService fileAnalysisService,
        [Description("Relative path to file")] string path,
        [Description("Line number to center on (1-based)")] int centerLine,
        [Description("Number of lines to include before and after center line")] int contextLines = 5)
    {
        return await fileAnalysisService.ReadAroundLineAsync(path, centerLine, contextLines);
    }

    [McpServerTool]
    [Description("Search for text in files with surrounding context")]
    public static async Task<string> SearchFilesWithContext(
        IFileAnalysisService fileAnalysisService,
        [Description("Text to search for")] string text,
        [Description("Path to search in (default: root)")] string path = "",
        [Description("Number of lines to include before and after match")] int contextLines = 3,
        [Description("File pattern filter (e.g., \"*.cs\")")] string filePattern = "*",
        [Description("Maximum number of results to return")] int maxResults = 20)
    {
        return await fileAnalysisService.SearchFilesWithContextAsync(text, path, contextLines, filePattern, maxResults);
    }

    [McpServerTool]
    [Description("Get method signatures from C# files without implementation details")]
    public static async Task<string> GetMethodSignatures(
        IFileAnalysisService fileAnalysisService,
        [Description("Relative path to .cs file")] string path,
        [Description("Optional: specific class name to analyze")] string? className = null,
        [Description("Include property signatures")] bool includeProperties = true)
    {
        return await fileAnalysisService.GetMethodSignaturesAsync(path, className, includeProperties);
    }

    [McpServerTool]
    [Description("Generate structured directory overview with filtering")]
    public static async Task<string> FileTreeSummary(
        IFileAnalysisService fileAnalysisService,
        [Description("Path to analyze (default: root)")] string path = "",
        [Description("Maximum directory depth to traverse")] int maxDepth = 3,
        [Description("File extensions to include (e.g., \"cs,json,md\")")] string fileTypes = "",
        [Description("Include hidden files and directories")] bool includeHidden = false,
        [Description("Include file sizes and line counts")] bool includeDetails = true,
        [Description("Sort files by: name, size, modified, extension")] string sortBy = "name")
    {
        return await fileAnalysisService.GetFileTreeSummaryAsync(path, maxDepth, fileTypes, includeHidden, includeDetails, sortBy);
    }
}
