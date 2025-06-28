using System.ComponentModel;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Aspects;
using CodeEditor.MCP.Models;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
[ToolLoggingAspect]
public static class FileTools
{
    [McpServerTool]
    [Description(
        "Lists all files and directories in the specified path with optional pattern filtering and detailed file information.")]
    public static Models.FileInfo[] ListFiles(
        IFileService service,
        [Description("Directory path to list (defaults to current directory)")]
        string path = ".",
        [Description("File pattern filter (e.g. '*.cs' for C# files, '*.js,*.ts' for multiple types)")]
        string? filter = null)
    {
        return service.ListFiles(path, filter);
    }

    [McpServerTool]
    [Description("Reads and returns the complete contents of a text file.")]
    public static string ReadFile(
        IFileService service,
        [Description("Path to the file to read")]
        string path)
    {
        return service.ReadFile(path);
    }

    [McpServerTool]
    [Description("Creates or overwrites a file with the specified content.")]
    public static void WriteFile(
        IFileService service,
        [Description("Path where the file should be created or updated")]
        string path,
        [Description("Text content to write to the file")]
        string content)
    {
        service.WriteFile(path, content);
    }

    [McpServerTool]
    [Description("Permanently deletes one or more files or directories.")]
    public static void DeleteFile(
        IFileService service,
        [Description("List of file or directory paths to delete")]
        string[] paths)
    {
        service.DeleteFiles(paths);
    }

    [McpServerTool]
    [Description("Searches for text content within files, with optional pattern filtering.")]
    public static string[] SearchFiles(
        IFileService service,
        [Description("Text string to search for within file contents")]
        string text,
        [Description("Directory path to search in (defaults to current directory)")]
        string path = ".",
        [Description("File pattern filter to limit search scope (e.g. '*.cs,*.js')")]
        string? filter = null)
    {
        return service.SearchFiles(text, path, filter);
    }

    [McpServerTool]
    [Description("Copies files or directories using source-destination pairs.")]
    public static void CopyFile(
        IFileService service,
        [Description("Array of file operations, each containing source and destination paths")]
        FileOperation[] operations)
    {
        service.CopyFiles(operations);
    }

    [McpServerTool]
    [Description("Moves or renames files and directories using source-destination pairs.")]
    public static void MoveFile(
        IFileService service,
        [Description("Array of file operations, each containing source and destination paths")]
        FileOperation[] operations)
    {
        service.MoveFiles(operations);
    }

    [McpServerTool]
    [Description("Reads a specific range of lines from a file for targeted content viewing.")]
    public static string ReadFileRange(
        IFileService service,
        [Description("Path to the file to read")]
        string path,
        [Description("Starting line number (1-based)")]
        int startLine,
        [Description("Ending line number (1-based, inclusive)")]
        int endLine)
    {
        return service.ReadFileRange(path, startLine, endLine);
    }
}