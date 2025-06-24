using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services;

public interface IFileAnalysisService
{
    Task<string> ReadFileLinesAsync(string path, int startLine, int endLine);
    Task<string> ReadAroundLineAsync(string path, int centerLine, int contextLines = 5);
    Task<string> SearchFilesWithContextAsync(string text, string path = ".", int contextLines = 3, string filePattern = "*", int maxResults = 20);
    Task<string> GetMethodSignaturesAsync(string path, string? className = null, bool includeProperties = true);
    Task<string> GetFileTreeSummaryAsync(string path = ".", int maxDepth = 3, string fileTypes = "", bool includeHidden = false, bool includeDetails = true, string sortBy = "name");
}
