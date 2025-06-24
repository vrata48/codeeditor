namespace CodeEditor.MCP.Models;

public class BuildResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Errors { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public TimeSpan Duration { get; set; }
    public List<BuildError> ParsedErrors { get; set; } = new();
}
