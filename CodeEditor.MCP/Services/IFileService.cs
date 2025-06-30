using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services;

public interface IFileService
{
    Models.FileInfo[] ListFiles(string relativePath = ".", string? filter = null);
    string ReadFile(string relativePath, int? startLine = null, int? endLine = null);
    void WriteFile(string relativePath, string content);
    void DeleteFiles(string[] relativePaths);
    Models.FileInfo[] SearchFiles(string searchText, string relativePath = ".", string? filter = null);
    void CopyFiles(FileOperation[] operations);
    void MoveFiles(FileOperation[] operations);
}