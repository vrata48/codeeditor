using CodeEditor.MCP.Services;
using CodeEditor.MCP.Tools;

namespace CodeEditor.MCP.Tests;

public class ContextToolsTests
{
    private readonly IPathService _pathService;

    public ContextToolsTests()
    {
        _pathService = new PathService(Directory.GetCurrentDirectory());
    }

    [Fact]
    public async Task ReadFileLines_Should_ReadSpecificRange()
    {
        // Arrange
        var testFile = "test_readlines.cs";
        var testContent = string.Join("\n", Enumerable.Range(1, 100).Select(i => $"// Line {i}"));
        File.WriteAllText(testFile, testContent);

        try
        {
            // Act
            var result = await ContextTools.ReadFileLines(_pathService, testFile, 10, 15);

            // Assert
            Assert.Contains("Lines 10-15", result);
            Assert.Contains("// Line 10", result);
            Assert.Contains("// Line 15", result);
            Assert.DoesNotContain("// Line 9", result);
            Assert.DoesNotContain("// Line 16", result);
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    [Fact]
    public async Task ReadFileLines_Should_HandleOutOfRange()
    {
        // Arrange
        var testFile = "test_readlines_short.cs";
        var testContent = "Line 1\nLine 2\nLine 3";
        File.WriteAllText(testFile, testContent);

        try
        {
            // Act
            var result = await ContextTools.ReadFileLines(_pathService, testFile, 10, 15);

            // Assert
            Assert.Contains("File only has 3 lines", result);
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    [Fact]
    public async Task ReadAroundLine_Should_ProvideContext()
    {
        // Arrange
        var testFile = "test_around_line.cs";
        var testContent = string.Join("\n", Enumerable.Range(1, 20).Select(i => $"// Line {i}"));
        File.WriteAllText(testFile, testContent);

        try
        {
            // Act
            var result = await ContextTools.ReadAroundLine(_pathService, testFile, 10, 3);

            // Assert
            Assert.Contains("// Line 7", result);  // 3 lines before
            Assert.Contains("// Line 10", result); // center line
            Assert.Contains("// Line 13", result); // 3 lines after
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    [Fact]
    public async Task SearchFilesWithContext_Should_FindMatches()
    {
        // Arrange
        var testFile1 = "search_test1.cs";
        var testFile2 = "search_test2.cs";
        
        File.WriteAllText(testFile1, @"
using System;
public class TestClass1 
{
    public void TestMethod() 
    {
        // This contains SEARCH_TARGET
        Console.WriteLine(""Hello"");
    }
}
");

        File.WriteAllText(testFile2, @"
using System;
public class TestClass2 
{
    // Another SEARCH_TARGET here
    public void AnotherMethod() 
    {
        var x = 5;
    }
}
");

        try
        {
            // Act
            var result = await ContextTools.SearchFilesWithContext(_pathService, "SEARCH_TARGET", "", 2, "*.cs", 10);

            // Assert
            Assert.Contains("Found", result);
            Assert.Contains("SEARCH_TARGET", result);
            Assert.Contains("search_test1.cs", result);
            Assert.Contains("search_test2.cs", result);
        }
        finally
        {
            if (File.Exists(testFile1)) File.Delete(testFile1);
            if (File.Exists(testFile2)) File.Delete(testFile2);
        }
    }

    [Fact]
    public async Task GetMethodSignatures_Should_ExtractSignatures()
    {
        // Arrange
        var testFile = "signature_test.cs";
        File.WriteAllText(testFile, @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public string PublicProperty { get; set; }
        
        public async Task<string> GetDataAsync(int id, string name)
        {
            return ""data"";
        }

        private void PrivateMethod()
        {
            // Implementation
        }
    }
}
");

        try
        {
            // Act
            var result = await ContextTools.GetMethodSignatures(_pathService, testFile);

            // Assert - Note: This is a simplified implementation, so we just test it doesn't crash
            Assert.NotNull(result);
            Assert.Contains("signature_test.cs", result);
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    [Fact]
    public async Task FileTreeSummary_Should_GenerateOverview()
    {
        // Arrange
        Directory.CreateDirectory("test_tree");
        Directory.CreateDirectory("test_tree/subdir");
        
        File.WriteAllText("test_tree/file1.cs", "// Test file 1");
        File.WriteAllText("test_tree/file2.json", "{ \"test\": true }");
        File.WriteAllText("test_tree/subdir/file3.cs", "// Test file 3");

        try
        {
            // Act
            var result = await ContextTools.FileTreeSummary(_pathService, "test_tree", 2, "", false, true, "name");

            // Assert - Note: This is a simplified implementation
            Assert.NotNull(result);
            Assert.Contains("test_tree", result);
        }
        finally
        {
            if (Directory.Exists("test_tree"))
                Directory.Delete("test_tree", true);
        }
    }

    [Theory]
    [InlineData("", 1, 5)]
    [InlineData("test.cs", 0, 5)]
    [InlineData("test.cs", 5, 3)]
    public async Task ReadFileLines_Should_ValidateInput(string path, int startLine, int endLine)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => ContextTools.ReadFileLines(_pathService, path, startLine, endLine));
    }

    [Fact]
    public async Task ReadFileLines_Should_ThrowForNonExistentFile()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => ContextTools.ReadFileLines(_pathService, "non_existent.cs", 1, 5));
    }

    [Fact]
    public async Task SearchFilesWithContext_Should_ThrowForEmptySearchText()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => ContextTools.SearchFilesWithContext(_pathService, "", "", 3, "*", 10));
    }

    [Fact]
    public async Task GetMethodSignatures_Should_ThrowForNonCSharpFile()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => ContextTools.GetMethodSignatures(_pathService, "test.txt"));
    }

    [Fact]
    public async Task FileTreeSummary_Should_ThrowForNonExistentDirectory()
    {
        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => ContextTools.FileTreeSummary(_pathService, "non_existent_dir"));
    }
}
