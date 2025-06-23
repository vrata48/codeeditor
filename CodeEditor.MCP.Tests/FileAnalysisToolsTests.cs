using CodeEditor.MCP.Services;
using CodeEditor.MCP.Tools;
using Moq;

namespace CodeEditor.MCP.Tests;

public class FileAnalysisToolsTests
{
    private readonly Mock<IFileAnalysisService> _mockFileAnalysisService;

    public FileAnalysisToolsTests()
    {
        _mockFileAnalysisService = new Mock<IFileAnalysisService>();
    }

    [Fact]
    public async Task ReadFileLines_Should_CallService()
    {
        // Arrange
        var expectedResult = "Lines 10-15 of 100 total lines from test.cs\n// Line 10\n// Line 11";
        _mockFileAnalysisService.Setup(s => s.ReadFileLinesAsync("test.cs", 10, 15))
                          .ReturnsAsync(expectedResult);

        // Act
        var result = await FileAnalysisTools.ReadFileLines(_mockFileAnalysisService.Object, "test.cs", 10, 15);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockFileAnalysisService.Verify(s => s.ReadFileLinesAsync("test.cs", 10, 15), Times.Once);
    }

    [Fact]
    public async Task ReadAroundLine_Should_CallService()
    {
        // Arrange
        var expectedResult = "Lines 7-13 of 100 total lines from test.cs\n// Line 7\n// Line 10\n// Line 13";
        _mockFileAnalysisService.Setup(s => s.ReadAroundLineAsync("test.cs", 10, 3))
                          .ReturnsAsync(expectedResult);

        // Act
        var result = await FileAnalysisTools.ReadAroundLine(_mockFileAnalysisService.Object, "test.cs", 10, 3);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockFileAnalysisService.Verify(s => s.ReadAroundLineAsync("test.cs", 10, 3), Times.Once);
    }

    [Fact]
    public async Task SearchFilesWithContext_Should_CallService()
    {
        // Arrange
        var expectedResult = "Found 2 matches for 'SEARCH_TARGET':\n\ntest1.cs:7\n...";
        _mockFileAnalysisService.Setup(s => s.SearchFilesWithContextAsync("SEARCH_TARGET", "", 3, "*.cs", 20))
                          .ReturnsAsync(expectedResult);

        // Act
        var result = await FileAnalysisTools.SearchFilesWithContext(_mockFileAnalysisService.Object, "SEARCH_TARGET", "", 3, "*.cs", 20);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockFileAnalysisService.Verify(s => s.SearchFilesWithContextAsync("SEARCH_TARGET", "", 3, "*.cs", 20), Times.Once);
    }

    [Fact]
    public async Task GetMethodSignatures_Should_CallService()
    {
        // Arrange
        var expectedResult = "Method signatures from test.cs\n================\n\nMETHODS:\n--------\n    public GetDataAsync() // Line 10";
        _mockFileAnalysisService.Setup(s => s.GetMethodSignaturesAsync("test.cs", null, true))
                          .ReturnsAsync(expectedResult);

        // Act
        var result = await FileAnalysisTools.GetMethodSignatures(_mockFileAnalysisService.Object, "test.cs", null, true);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockFileAnalysisService.Verify(s => s.GetMethodSignaturesAsync("test.cs", null, true), Times.Once);
    }

    [Fact]
    public async Task FileTreeSummary_Should_CallService()
    {
        // Arrange
        var expectedResult = "📁 Directory Tree Summary: test_tree\n==================================================\nTotal Files: 3\nTotal Size: 1.2 KB";
        _mockFileAnalysisService.Setup(s => s.GetFileTreeSummaryAsync("test_tree", 3, "", false, true, "name"))
                          .ReturnsAsync(expectedResult);

        // Act
        var result = await FileAnalysisTools.FileTreeSummary(_mockFileAnalysisService.Object, "test_tree", 3, "", false, true, "name");

        // Assert
        Assert.Equal(expectedResult, result);
        _mockFileAnalysisService.Verify(s => s.GetFileTreeSummaryAsync("test_tree", 3, "", false, true, "name"), Times.Once);
    }
}
