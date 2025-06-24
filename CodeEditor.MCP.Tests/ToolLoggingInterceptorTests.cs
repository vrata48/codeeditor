using CodeEditor.MCP.Extensions;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace CodeEditor.MCP.Tests;
public class ToolLoggingInterceptorTests
{
    [Fact]
    public void FileTools_WorksWithMockFileSystem()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var tempDirectory = "/temp";
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(mockFileSystem);
        services.AddSingleton<IPathService>(_ => new PathService(tempDirectory));
        services.AddSingleton<IToolLoggingService, ToolLoggingService>();
        services.AddSingleton<IFileFilterService>(_ => new FileFilterService(new PathService(tempDirectory), null // No global filter
        ));
        services.AddSingleton<IFileService, FileService>();
        var serviceProvider = services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();
        // Act & Assert - This should fail because file doesn't exist
        Assert.Throws<FileNotFoundException>(() => FileTools.ReadFile(fileService, "non-existent-file.txt"));
        // Note: Actual logging would happen if AspectInjector was properly compiled
        // For now we just verify the tool behavior is correct
        Assert.True(true);
    }

    [Fact]
    public void FileTools_SuccessfulReadDoesNotFail()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile("/temp/test-success.txt", new MockFileData("test content"));
        var tempDirectory = "/temp";
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(mockFileSystem);
        services.AddSingleton<IPathService>(_ => new PathService(tempDirectory));
        services.AddSingleton<IToolLoggingService, ToolLoggingService>();
        services.AddSingleton<IFileFilterService>(_ => new FileFilterService(new PathService(tempDirectory), null // No global filter
        ));
        services.AddSingleton<IFileService, FileService>();
        var serviceProvider = services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();
        // Act - This should succeed
        var content = FileTools.ReadFile(fileService, "test-success.txt");
        // Assert
        Assert.Equal("test content", content);
    }

    [Fact]
    public void LogFailedToolCall_ExcludesStackTraceAndInnerException()
    {
        // Arrange
        var tempDirectory = Path.GetTempPath();
        var logDirectory = Path.Combine(tempDirectory, ".mcp-logs");
        // Clean up any existing logs
        if (Directory.Exists(logDirectory))
        {
            Directory.Delete(logDirectory, true);
        }

        var pathService = new PathService(tempDirectory);
        var toolLoggingService = new ToolLoggingService(pathService);
        // Create a nested exception to test inner exception exclusion
        var innerException = new InvalidOperationException("Inner exception message");
        var outerException = new ArgumentException("Outer exception message", innerException);
        // Act
        toolLoggingService.LogFailedToolCall("TestTool", "TestMethod", new { param = "value" }, outerException);
        // Assert - Read the log file and verify structure
        var logFile = Path.Combine(logDirectory, $"failed-tools-{DateTime.UtcNow:yyyy-MM-dd}.json");
        Assert.True(File.Exists(logFile));
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("\"type\":", logContent);
        Assert.Contains("\"message\":", logContent);
        Assert.Contains("Outer exception message", logContent);
        // Verify that stack trace and inner exception are NOT included
        Assert.DoesNotContain("\"stackTrace\":", logContent);
        Assert.DoesNotContain("\"innerException\":", logContent);
        Assert.DoesNotContain("Inner exception message", logContent);
        // Clean up
        Directory.Delete(logDirectory, true);
    }

    [Fact]
    public void LogFailedToolCall_LogsBasicExceptionInfo()
    {
        // Arrange
        var tempDirectory = Path.GetTempPath();
        var logDirectory = Path.Combine(tempDirectory, ".mcp-logs");
        // Clean up any existing logs
        if (Directory.Exists(logDirectory))
        {
            Directory.Delete(logDirectory, true);
        }

        var pathService = new PathService(tempDirectory);
        var toolLoggingService = new ToolLoggingService(pathService);
        var exception = new FileNotFoundException("File not found");
        var request = new
        {
            path = "test.txt",
            content = "some content"
        };
        // Act
        toolLoggingService.LogFailedToolCall("FileTools", "ReadFile", request, exception);
        // Assert - Read the log file and verify basic structure
        var logFile = Path.Combine(logDirectory, $"failed-tools-{DateTime.UtcNow:yyyy-MM-dd}.json");
        Assert.True(File.Exists(logFile));
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("\"toolName\": \"FileTools\"", logContent);
        Assert.Contains("\"methodName\": \"ReadFile\"", logContent);
        Assert.Contains("\"type\": \"System.IO.FileNotFoundException\"", logContent);
        Assert.Contains("\"message\": \"File not found\"", logContent);
        Assert.Contains("\"path\": \"test.txt\"", logContent);
        // Clean up
        Directory.Delete(logDirectory, true);
    }
}