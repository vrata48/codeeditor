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
