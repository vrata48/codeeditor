using System.ComponentModel;
using CodeEditor.MCP.Services;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
public static class DocumentFormattingTools
{
    [McpServerTool]
    [Description("Format a C# document using Roslyn formatting rules")]
    public static string FormatDocument(
        IDocumentFormattingService service,
        [Description("Relative path to .cs file")] string path)
    {
        return service.FormatDocument(path);
    }

    [McpServerTool]
    [Description("Format multiple C# documents in a directory")]
    public static string FormatDirectory(
        IDocumentFormattingService service,
        [Description("Relative path to directory containing .cs files")] string path,
        [Description("Whether to format files in subdirectories")] bool recursive = false)
    {
        return service.FormatDirectory(path, recursive);
    }

    [McpServerTool]
    [Description("Validate if a document has proper formatting")]
    public static string ValidateFormatting(
        IDocumentFormattingService service,
        [Description("Relative path to .cs file")] string path)
    {
        return service.ValidateFormatting(path);
    }
}
