namespace CodeEditor.MCP.Models;

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
