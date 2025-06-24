namespace CodeEditor.MCP.Models;

public class FailedTest
{
    public string TestName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
}
