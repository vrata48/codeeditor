namespace CodeEditor.MCP.Models;

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
