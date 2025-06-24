namespace CodeEditor.MCP.Services;

public interface IFileService
{
string[] ListFiles(string relativePath = ".");     string ReadFile(string relativePath);
    void WriteFile(string relativePath, string content);
    void DeleteFile(string relativePath);
string[] SearchFiles(string searchText, string relativePath = ".");     void CopyFile(string sourceRelativePath, string destinationRelativePath);
    void MoveFile(string sourceRelativePath, string destinationRelativePath);
}
