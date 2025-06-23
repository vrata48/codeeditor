namespace CodeEditor.MCP.Models;

public class SearchResult
{
    public string FilePath { get; set; } = "";
    public int LineNumber { get; set; }
    public string MatchLine { get; set; } = "";
    public List<string> ContextBefore { get; set; } = new();
    public List<string> ContextAfter { get; set; } = new();
    public string MatchedText { get; set; } = "";
}

public class MethodSignature
{
    public string Name { get; set; } = "";
    public string ReturnType { get; set; } = "";
    public List<string> Parameters { get; set; } = new();
    public string AccessModifier { get; set; } = "";
    public List<string> Modifiers { get; set; } = new();
    public string FullSignature { get; set; } = "";
    public int LineNumber { get; set; }
    public string ClassName { get; set; } = "";
    public List<string> Attributes { get; set; } = new();
}

public class PropertySignature
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string AccessModifier { get; set; } = "";
    public List<string> Modifiers { get; set; } = new();
    public string Accessors { get; set; } = "";
    public int LineNumber { get; set; }
    public List<string> Attributes { get; set; } = new();
}

public class DirectoryInfo
{
    public string Name { get; set; } = "";
    public string RelativePath { get; set; } = "";
    public List<FileInfo> Files { get; set; } = new();
    public List<DirectoryInfo> Subdirectories { get; set; } = new();
    public int TotalFiles { get; set; }
    public long TotalSize { get; set; }
}

public class FileInfo
{
    public string Name { get; set; } = "";
    public string RelativePath { get; set; } = "";
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string Extension { get; set; } = "";
    public int LineCount { get; set; }
}
