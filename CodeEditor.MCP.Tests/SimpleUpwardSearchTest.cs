using System.IO.Abstractions;
using CodeEditor.MCP.Services;
using FluentAssertions;

namespace CodeEditor.MCP.Tests;

public class SimpleUpwardSearchTest : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _subDirectory;

    public SimpleUpwardSearchTest()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _subDirectory = Path.Combine(_tempDirectory, "subfolder");
        Directory.CreateDirectory(_subDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void PathService_SearchesUpwardForGitignore_SimpleTest()
    {
        // Arrange - Create .gitignore in parent directory
        var gitignoreContent = "*.log";
        File.WriteAllText(Path.Combine(_tempDirectory, ".gitignore"), gitignoreContent);
        
        // Create PathService from subdirectory
        var pathService = new PathService(_subDirectory);

        // Act & Assert - Pattern from parent .gitignore should work
        pathService.ShouldIgnore("test.log").Should().BeTrue("*.log pattern should be found from parent .gitignore");
        pathService.ShouldIgnore("test.txt").Should().BeFalse("Non-matching files should not be ignored");
    }
}
