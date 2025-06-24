namespace CodeEditor.MCP.Models;

public class TestResult : BuildResult
{
    public int TestsPassed { get; set; }
    public int TestsFailed { get; set; }
    public int TestsSkipped { get; set; }
    public int TotalTests { get; set; }
    public List<FailedTest> FailedTests { get; set; } = new();
public int Passed => TestsPassed; public int Failed => TestsFailed; public int Skipped => TestsSkipped; }
