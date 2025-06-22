namespace CodeEditor.MCP.Models;

public class TestResult : BuildResult
{
    public int TestsPassed { get; set; }
    public int TestsFailed { get; set; }
    public int TestsSkipped { get; set; }
    public int TotalTests { get; set; }
    public List<FailedTest> FailedTests { get; set; } = new();
}

public class FailedTest
{
    public string TestName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
}
