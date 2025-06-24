namespace CodeEditor.MCP.Models;

public class DirectoryInfo
{
    public string Name { get; set; } = "";
    public string RelativePath { get; set; } = "";
    public List<FileInfo> Files { get; set; } = new();
    public List<DirectoryInfo> Subdirectories { get; set; } = new();
    public int TotalFiles { get; set; }
    public long TotalSize { get; set; }
}
