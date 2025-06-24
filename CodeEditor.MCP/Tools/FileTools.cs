using System.ComponentModel;
using CodeEditor.MCP.Services;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
public static class FileTools
{
[McpServerTool]
    [Description("List files and folders.")]
    public static string[] ListFiles(
        IFileService service,
        [Description("Path to list (default: root).")] string path = ".")
    {
        return service.ListFiles(path);
    }     
    [McpServerTool]
    [Description("Read file content.")]
    public static string ReadFile(
        IFileService service,
        [Description("Path to file.")] string path)
    {
        return service.ReadFile(path);
    }
    
    [McpServerTool]
    [Description("Write file content.")]
    public static void WriteFile(
        IFileService service,
        [Description("Path to file.")] string path,
        [Description("File content.")] string content)
    {
        service.WriteFile(path, content);
    }
    
    [McpServerTool]
    [Description("Delete file or folder.")]
    public static void DeleteFile(
        IFileService service,
        [Description("Path to delete.")] string path)
    {
        service.DeleteFile(path);
    }
[McpServerTool]
    [Description("Search for text in files.")]
    public static string[] SearchFiles(
        IFileService service,
        [Description("Text to search for.")] string text,
        [Description("Path to search in (default: root).")] string path = ".")
    {
        return service.SearchFiles(text, path);
    }     
    [McpServerTool]
    [Description("Copy file or folder.")]
    public static void CopyFile(
        IFileService service,
        [Description("Source Path.")] string source,
        [Description("Destination Path.")] string destination)
    {
        service.CopyFile(source, destination);
    }
    
    [McpServerTool]
    [Description("Move or rename file or folder.")]
    public static void MoveFile(
        IFileService service,
        [Description("Source Path.")] string source,
        [Description("Destination Path.")] string destination)
    {
        service.MoveFile(source, destination);
    }
}
