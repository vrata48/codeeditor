namespace CodeEditor.MCP.Services;

public interface ICSharpFormattingService
{
    /// <summary>
    /// Formats a C# document using Roslyn formatting rules
    /// </summary>
    /// <param name="relativePath">Relative path to the .cs file</param>
    /// <returns>Formatted document content</returns>
    string FormatDocument(string relativePath);
    
    /// <summary>
    /// Formats multiple C# documents in a directory
    /// </summary>
    /// <param name="relativePath">Relative path to directory containing .cs files</param>
    /// <param name="recursive">Whether to format files in subdirectories</param>
    /// <returns>Summary of formatting results</returns>
    string FormatDirectory(string relativePath, bool recursive = false);
}
