using System.IO.Abstractions.TestingHelpers;
using CodeEditor.MCP.Services;
using FluentAssertions;

namespace CodeEditor.MCP.Tests;

public class FileTreeSummaryBasicTests
{
    [Fact]
    public async Task FileTreeSummary_WithBasicStructure_WorksCorrectly()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Create basic structure
            File.WriteAllText(Path.Combine(tempDir, "test.cs"), "// Test file");
            Directory.CreateDirectory(Path.Combine(tempDir, "src"));
            File.WriteAllText(Path.Combine(tempDir, "src", "program.cs"), "// Program");
            
            var pathService = new PathService(tempDir);
            var fileAnalysisService = new FileAnalysisService(pathService);

            // Act
            var result = await fileAnalysisService.GetFileTreeSummaryAsync(".", 2, "cs", false, true, "name");

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("test.cs");
            result.Should().Contain("program.cs");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
