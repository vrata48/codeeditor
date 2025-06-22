using System.Text.Json;
using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Extensions;

public static class ResultExtensions
{
    public static string ToFormattedJson(this BuildResult result)
    {
        if (result.Success)
        {
            var successResult = new
            {
                success = true,
                exitCode = result.ExitCode,
                errorCount = 0
            };

            return JsonSerializer.Serialize(successResult, new JsonSerializerOptions { WriteIndented = true });
        }
        else
        {
            var failureResult = new
            {
                success = false,
                exitCode = result.ExitCode,
                errorCount = result.ParsedErrors.Count,
                output = !string.IsNullOrEmpty(result.Output) ? result.Output : null,
                errors = !string.IsNullOrEmpty(result.Errors) ? result.Errors : null,
                parsedErrors = result.ParsedErrors.Any() ? result.ParsedErrors.Select(e => new
                {
                    severity = e.Severity,
                    errorCode = e.ErrorCode,
                    message = e.Message,
                    file = e.File,
                    line = e.Line,
                    column = e.Column
                }).ToList() : null
            };

            return JsonSerializer.Serialize(failureResult, new JsonSerializerOptions { 
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
    }

    public static string ToFormattedJson(this TestResult result)
    {
        if (result.Success)
        {
            var successResult = new
            {
                success = true,
                exitCode = result.ExitCode,
                totalTests = result.TotalTests,
                passed = result.TestsPassed,
                failed = result.TestsFailed,
                skipped = result.TestsSkipped
            };

            return JsonSerializer.Serialize(successResult, new JsonSerializerOptions { WriteIndented = true });
        }
        else
        {
            var failureResult = new
            {
                success = false,
                exitCode = result.ExitCode,
                totalTests = result.TotalTests,
                passed = result.TestsPassed,
                failed = result.TestsFailed,
                skipped = result.TestsSkipped,
                output = !string.IsNullOrEmpty(result.Output) ? result.Output : null,
                errors = !string.IsNullOrEmpty(result.Errors) ? result.Errors : null,
                failedTests = result.FailedTests.Any() ? result.FailedTests.Select(t => new
                {
                    testName = t.TestName,
                    className = t.ClassName,
                    errorMessage = t.ErrorMessage,
                    stackTrace = !string.IsNullOrEmpty(t.StackTrace) ? t.StackTrace : null
                }).ToList() : null
            };

            return JsonSerializer.Serialize(failureResult, new JsonSerializerOptions { 
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
    }
}
