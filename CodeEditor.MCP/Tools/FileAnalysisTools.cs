using System.ComponentModel;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Aspects;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
[ToolLoggingAspect] // Apply logging aspect to all methods in this class
public static class FileAnalysisTools
{
public static async Task<string> FileTreeSummary(
        IFileAnalysisService service,
        [Description("Path to analyze (default: root)")] string path = ".",
        [Description("Maximum directory depth to traverse")] int maxDepth = 3,
        [Description("File extensions to include (e.g., \"cs,json,md\")")] string fileTypes = "",
        [Description("Include hidden files and directories")] bool includeHidden = false,
        [Description("Include file sizes and line counts")] bool includeDetails = true,
        [Description("Sort files by: name, size, modified, extension")] string sortBy = "name")
    {
        return await service.GetFileTreeSummaryAsync(path, maxDepth, fileTypes, includeHidden, includeDetails, sortBy);
    } public static async Task<string> ReadFileLines(
        IFileAnalysisService service,
        [Description("Relative path to file")] string path,
        [Description("Starting line number (1-based)")] int startLine,
        [Description("Ending line number (1-based, inclusive)")] int endLine)
    {
        return await service.ReadFileLinesAsync(path, startLine, endLine);
    } public static async Task<string> ReadAroundLine(
        IFileAnalysisService service,
        [Description("Relative path to file")] string path,
        [Description("Line number to center on (1-based)")] int centerLine,
        [Description("Number of lines to include before and after center line")] int contextLines = 5)
    {
        return await service.ReadAroundLineAsync(path, centerLine, contextLines);
    } public static async Task<string> SearchFilesWithContext(
        IFileAnalysisService service,
        [Description("Text to search for")] string text,
        [Description("Path to search in (default: root)")] string path = ".",
        [Description("Number of lines to include before and after match")] int contextLines = 3,
        [Description("File pattern filter (e.g., \"*.cs\")")] string filePattern = "*",
        [Description("Maximum number of results to return")] int maxResults = 20)
    {
        return await service.SearchFilesWithContextAsync(text, path, contextLines, filePattern, maxResults);
    } public static async Task<string> GetMethodSignatures(
        IFileAnalysisService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Optional: specific class name to analyze")] string? className = null,
        [Description("Include property signatures")] bool includeProperties = true)
    {
        return await service.GetMethodSignaturesAsync(path, className, includeProperties);
    } }
