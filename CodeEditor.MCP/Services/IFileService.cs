using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services;

public interface IFileService
{
Models.FileInfo[] ListFiles(string relativePath = ".", string? filter = null);     string ReadFile(string relativePath);
    void WriteFile(string relativePath, string content);
    void DeleteFiles(string[] relativePaths);
    string[] SearchFiles(string searchText, string relativePath = ".", string? filter = null);
    void CopyFiles(FileOperation[] operations);
    void MoveFiles(FileOperation[] operations);
string ReadFileRange(string relativePath, int startLine, int endLine); }
