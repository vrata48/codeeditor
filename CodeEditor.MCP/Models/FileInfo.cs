namespace CodeEditor.MCP.Models;

public class FileInfo
{
    public string Name { get; set; } = "";
    public string RelativePath { get; set; } = "";
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string Extension { get; set; } = "";
    public int LineCount { get; set; }
}
