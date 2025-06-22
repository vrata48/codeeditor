using System.Text.Json;
using CodeEditor.MCP.Extensions;
using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Tests;

public class ResultExtensionsTests
{
    [Fact]
    public void BuildResult_ToFormattedJson_WhenSuccess_ReturnsMinimalJson()
    {
        // Arrange
        var result = new BuildResult
        {
            Success = true,
            ExitCode = 0,
            Output = "Build succeeded",
            Errors = "",
            Duration = TimeSpan.FromSeconds(5),
            ParsedErrors = new List<BuildError>()
        };

        // Act
        var json = result.ToFormattedJson();
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.True(parsed.GetProperty("success").GetBoolean());
        Assert.Equal(0, parsed.GetProperty("exitCode").GetInt32());
        Assert.Equal(0, parsed.GetProperty("errorCount").GetInt32());
        
        // Should not contain detailed information
        Assert.False(parsed.TryGetProperty("output", out _));
        Assert.False(parsed.TryGetProperty("errors", out _));
        Assert.False(parsed.TryGetProperty("parsedErrors", out _));
        Assert.False(parsed.TryGetProperty("duration", out _));
    }

    [Fact]
    public void BuildResult_ToFormattedJson_WhenFailure_ReturnsDetailedJson()
    {
        // Arrange
        var result = new BuildResult
        {
            Success = false,
            ExitCode = 1,
            Output = "Build failed with errors",
            Errors = "CS1001: Identifier expected",
            Duration = TimeSpan.FromSeconds(5),
            ParsedErrors = new List<BuildError>
            {
                new BuildError
                {
                    File = "Program.cs",
                    Line = 10,
                    Column = 5,
                    Severity = "error",
                    ErrorCode = "CS1001",
                    Message = "Identifier expected"
                }
            }
        };

        // Act
        var json = result.ToFormattedJson();
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.False(parsed.GetProperty("success").GetBoolean());
        Assert.Equal(1, parsed.GetProperty("exitCode").GetInt32());
        Assert.Equal(1, parsed.GetProperty("errorCount").GetInt32());
        Assert.Equal("Build failed with errors", parsed.GetProperty("output").GetString());
        Assert.Equal("CS1001: Identifier expected", parsed.GetProperty("errors").GetString());
        
        var parsedErrors = parsed.GetProperty("parsedErrors").EnumerateArray().ToList();
        Assert.Single(parsedErrors);
        Assert.Equal("error", parsedErrors[0].GetProperty("severity").GetString());
        Assert.Equal("CS1001", parsedErrors[0].GetProperty("errorCode").GetString());
        Assert.Equal("Program.cs", parsedErrors[0].GetProperty("file").GetString());
        Assert.Equal(10, parsedErrors[0].GetProperty("line").GetInt32());
        Assert.Equal(5, parsedErrors[0].GetProperty("column").GetInt32());
        
        // Should not contain duration
        Assert.False(parsed.TryGetProperty("duration", out _));
    }

    [Fact]
    public void BuildResult_ToFormattedJson_WhenFailureWithEmptyStrings_OmitsNullProperties()
    {
        // Arrange
        var result = new BuildResult
        {
            Success = false,
            ExitCode = 1,
            Output = "",
            Errors = "",
            ParsedErrors = new List<BuildError>()
        };

        // Act
        var json = result.ToFormattedJson();
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.False(parsed.GetProperty("success").GetBoolean());
        Assert.Equal(1, parsed.GetProperty("exitCode").GetInt32());
        Assert.Equal(0, parsed.GetProperty("errorCount").GetInt32());
        
        // Should not contain empty/null properties
        Assert.False(parsed.TryGetProperty("output", out _));
        Assert.False(parsed.TryGetProperty("errors", out _));
        Assert.False(parsed.TryGetProperty("parsedErrors", out _));
    }

    [Fact]
    public void TestResult_ToFormattedJson_WhenSuccess_ReturnsTestSummary()
    {
        // Arrange
        var result = new TestResult
        {
            Success = true,
            ExitCode = 0,
            Output = "All tests passed",
            Errors = "",
            Duration = TimeSpan.FromSeconds(10),
            TestsPassed = 5,
            TestsFailed = 0,
            TestsSkipped = 1,
            TotalTests = 6,
            FailedTests = new List<FailedTest>()
        };

        // Act
        var json = result.ToFormattedJson();
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.True(parsed.GetProperty("success").GetBoolean());
        Assert.Equal(0, parsed.GetProperty("exitCode").GetInt32());
        Assert.Equal(6, parsed.GetProperty("totalTests").GetInt32());
        Assert.Equal(5, parsed.GetProperty("passed").GetInt32());
        Assert.Equal(0, parsed.GetProperty("failed").GetInt32());
        Assert.Equal(1, parsed.GetProperty("skipped").GetInt32());
        
        // Should not contain detailed information
        Assert.False(parsed.TryGetProperty("output", out _));
        Assert.False(parsed.TryGetProperty("errors", out _));
        Assert.False(parsed.TryGetProperty("failedTests", out _));
        Assert.False(parsed.TryGetProperty("duration", out _));
    }

    [Fact]
    public void TestResult_ToFormattedJson_WhenFailure_ReturnsDetailedJson()
    {
        // Arrange
        var result = new TestResult
        {
            Success = false,
            ExitCode = 1,
            Output = "Test run failed",
            Errors = "Test execution error",
            Duration = TimeSpan.FromSeconds(10),
            TestsPassed = 2,
            TestsFailed = 1,
            TestsSkipped = 0,
            TotalTests = 3,
            FailedTests = new List<FailedTest>
            {
                new FailedTest
                {
                    TestName = "MyTest.ShouldPass",
                    ClassName = "MyTest",
                    ErrorMessage = "Expected true but was false",
                    StackTrace = "at MyTest.ShouldPass() line 25"
                }
            }
        };

        // Act
        var json = result.ToFormattedJson();
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.False(parsed.GetProperty("success").GetBoolean());
        Assert.Equal(1, parsed.GetProperty("exitCode").GetInt32());
        Assert.Equal(3, parsed.GetProperty("totalTests").GetInt32());
        Assert.Equal(2, parsed.GetProperty("passed").GetInt32());
        Assert.Equal(1, parsed.GetProperty("failed").GetInt32());
        Assert.Equal(0, parsed.GetProperty("skipped").GetInt32());
        Assert.Equal("Test run failed", parsed.GetProperty("output").GetString());
        Assert.Equal("Test execution error", parsed.GetProperty("errors").GetString());
        
        var failedTests = parsed.GetProperty("failedTests").EnumerateArray().ToList();
        Assert.Single(failedTests);
        Assert.Equal("MyTest.ShouldPass", failedTests[0].GetProperty("testName").GetString());
        Assert.Equal("MyTest", failedTests[0].GetProperty("className").GetString());
        Assert.Equal("Expected true but was false", failedTests[0].GetProperty("errorMessage").GetString());
        Assert.Equal("at MyTest.ShouldPass() line 25", failedTests[0].GetProperty("stackTrace").GetString());
        
        // Should not contain duration
        Assert.False(parsed.TryGetProperty("duration", out _));
    }

    [Fact]
    public void TestResult_ToFormattedJson_WhenFailureWithEmptyStackTrace_OmitsStackTrace()
    {
        // Arrange
        var result = new TestResult
        {
            Success = false,
            ExitCode = 1,
            TestsPassed = 0,
            TestsFailed = 1,
            TestsSkipped = 0,
            TotalTests = 1,
            FailedTests = new List<FailedTest>
            {
                new FailedTest
                {
                    TestName = "MyTest.ShouldPass",
                    ClassName = "MyTest",
                    ErrorMessage = "Test failed",
                    StackTrace = "" // Empty stack trace
                }
            }
        };

        // Act
        var json = result.ToFormattedJson();
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        var failedTests = parsed.GetProperty("failedTests").EnumerateArray().ToList();
        Assert.Single(failedTests);
        Assert.Equal("MyTest.ShouldPass", failedTests[0].GetProperty("testName").GetString());
        Assert.Equal("MyTest", failedTests[0].GetProperty("className").GetString());
        Assert.Equal("Test failed", failedTests[0].GetProperty("errorMessage").GetString());
        
        // Should not contain empty stack trace
        Assert.False(failedTests[0].TryGetProperty("stackTrace", out _));
    }
}
