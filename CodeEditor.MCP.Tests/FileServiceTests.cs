using System.IO.Abstractions.TestingHelpers;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Models;
using FluentAssertions;

namespace CodeEditor.MCP.Tests;
public class FileServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testProjectDirectory;
    private PathService _pathService = null !;
    private FileService _fileService = null !;
    private MockFileSystem _fileSystem = null !;
    public FileServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _testProjectDirectory = Path.Combine(_tempDirectory, "TestProject");
        Directory.CreateDirectory(_testProjectDirectory);
        SetupServices();
    }

    private void SetupServices()
    {
        _pathService = new PathService(_testProjectDirectory);
        _fileSystem = new MockFileSystem();
        _fileSystem.AddDirectory(_testProjectDirectory);
        _fileService = new FileService(_fileSystem, _pathService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void ReadFile_ExistingFile_ReturnsContent()
    {
        // Arrange
        var relativePath = "test.txt";
        var content = "Hello, World!";
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        _fileSystem.AddFile(fullPath, new MockFileData(content));
        // Act
        var result = _fileService.ReadFile(relativePath);
        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void WriteFile_NewFile_CreatesFileWithContent()
    {
        // Arrange
        var relativePath = "new-file.txt";
        var content = "New content";
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        // Act
        _fileService.WriteFile(relativePath, content);
        // Assert
        _fileSystem.File.Exists(fullPath).Should().BeTrue();
        _fileSystem.File.ReadAllText(fullPath).Should().Be(content);
    }

    [Fact]
    public void WriteFile_FileInSubdirectory_CreatesDirectoryAndFile()
    {
        // Arrange
        var relativePath = "subdirectory/nested.txt";
        var content = "Nested content";
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        // Act
        _fileService.WriteFile(relativePath, content);
        // Assert
        _fileSystem.File.Exists(fullPath).Should().BeTrue();
        _fileSystem.File.ReadAllText(fullPath).Should().Be(content);
    }

    [Fact]
    public void DeleteFiles_SingleFile_RemovesFile()
    {
        // Arrange
        var relativePath = "to-delete.txt";
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        _fileSystem.AddFile(fullPath, new MockFileData("content"));
        // Act
        _fileService.DeleteFiles(new[] { relativePath });
        // Assert
        _fileSystem.File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public void DeleteFiles_SingleDirectory_RemovesDirectory()
    {
        // Arrange
        var relativePath = "dir-to-delete";
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        _fileSystem.AddDirectory(fullPath);
        _fileSystem.AddFile(Path.Combine(fullPath, "file.txt"), new MockFileData("content"));
        // Act
        _fileService.DeleteFiles(new[] { relativePath });
        // Assert
        _fileSystem.Directory.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public void SearchFiles_FindsFilesWithMatchingContent()
    {
        // Arrange
        var file1 = Path.Combine(_testProjectDirectory, "file1.txt");
        var file2 = Path.Combine(_testProjectDirectory, "file2.txt");
        var file3 = Path.Combine(_testProjectDirectory, "file3.txt");
        _fileSystem.AddFile(file1, new MockFileData("This contains the search term"));
        _fileSystem.AddFile(file2, new MockFileData("This does not contain it"));
        _fileSystem.AddFile(file3, new MockFileData("Another file with SEARCH TERM"));
        // Act
        var results = _fileService.SearchFiles("search term");
        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(f => f.RelativePath == "file1.txt");
        results.Should().Contain(f => f.RelativePath == "file3.txt");
        results.Should().NotContain(f => f.RelativePath == "file2.txt");
    }

    [Fact]
    public void SearchFiles_CaseInsensitive_FindsMatches()
    {
        // Arrange
        var file1 = Path.Combine(_testProjectDirectory, "file1.txt");
        _fileSystem.AddFile(file1, new MockFileData("UPPERCASE CONTENT"));
        // Act
        var results = _fileService.SearchFiles("uppercase");
        // Assert
        results.Should().Contain(f => f.RelativePath == "file1.txt");
    }

    [Fact]
    public void CopyFiles_SingleFile_CopiesSuccessfully()
    {
        // Arrange
        var sourceFile = Path.Combine(_testProjectDirectory, "source.txt");
        var destFile = Path.Combine(_testProjectDirectory, "destination.txt");
        var content = "File content";
        _fileSystem.AddFile(sourceFile, new MockFileData(content));
        // Act
        _fileService.CopyFiles(new[] { new FileOperation { Source = "source.txt", Destination = "destination.txt" } });
        // Assert
        _fileSystem.File.Exists(destFile).Should().BeTrue();
        _fileSystem.File.ReadAllText(destFile).Should().Be(content);
        _fileSystem.File.Exists(sourceFile).Should().BeTrue(); // Original should still exist
    }

    [Fact]
    public void CopyFiles_SingleDirectory_CopiesRecursively()
    {
        // Arrange
        var sourceDir = Path.Combine(_testProjectDirectory, "source-dir");
        var destDir = Path.Combine(_testProjectDirectory, "dest-dir");
        _fileSystem.AddDirectory(sourceDir);
        _fileSystem.AddFile(Path.Combine(sourceDir, "file1.txt"), new MockFileData("Content 1"));
        _fileSystem.AddDirectory(Path.Combine(sourceDir, "subdir"));
        _fileSystem.AddFile(Path.Combine(sourceDir, "subdir", "file2.txt"), new MockFileData("Content 2"));
        // Act
        _fileService.CopyFiles(new[] { new FileOperation { Source = "source-dir", Destination = "dest-dir" } });
        // Assert
        _fileSystem.Directory.Exists(destDir).Should().BeTrue();
        _fileSystem.File.Exists(Path.Combine(destDir, "file1.txt")).Should().BeTrue();
        _fileSystem.File.Exists(Path.Combine(destDir, "subdir", "file2.txt")).Should().BeTrue();
        _fileSystem.File.ReadAllText(Path.Combine(destDir, "file1.txt")).Should().Be("Content 1");
        _fileSystem.File.ReadAllText(Path.Combine(destDir, "subdir", "file2.txt")).Should().Be("Content 2");
    }

    [Fact]
    public void MoveFiles_SingleFile_MovesSuccessfully()
    {
        // Arrange
        var sourceFile = Path.Combine(_testProjectDirectory, "source.txt");
        var destFile = Path.Combine(_testProjectDirectory, "destination.txt");
        var content = "File content";
        _fileSystem.AddFile(sourceFile, new MockFileData(content));
        // Act
        _fileService.MoveFiles(new[] { new FileOperation { Source = "source.txt", Destination = "destination.txt" } });
        // Assert
        _fileSystem.File.Exists(destFile).Should().BeTrue();
        _fileSystem.File.ReadAllText(destFile).Should().Be(content);
        _fileSystem.File.Exists(sourceFile).Should().BeFalse(); // Original should be gone
    }

    [Fact]
    public void MoveFiles_SingleDirectory_MovesSuccessfully()
    {
        // Arrange
        var sourceDir = Path.Combine(_testProjectDirectory, "source-dir");
        var destDir = Path.Combine(_testProjectDirectory, "dest-dir");
        _fileSystem.AddDirectory(sourceDir);
        _fileSystem.AddFile(Path.Combine(sourceDir, "file.txt"), new MockFileData("Content"));
        // Act
        _fileService.MoveFiles(new[] { new FileOperation { Source = "source-dir", Destination = "dest-dir" } });
        // Assert
        _fileSystem.Directory.Exists(destDir).Should().BeTrue();
        _fileSystem.Directory.Exists(sourceDir).Should().BeFalse();
        _fileSystem.File.Exists(Path.Combine(destDir, "file.txt")).Should().BeTrue();
    }

    [Fact]
    public void ListFiles_WithNormalizedPaths_ReturnsForwardSlashes()
    {
        // Arrange
        var file1 = Path.Combine(_testProjectDirectory, "file1.txt");
        var subDir = Path.Combine(_testProjectDirectory, "subdir");
        var file2 = Path.Combine(subDir, "file2.txt");
        _fileSystem.AddFile(file1, new MockFileData("content"));
        _fileSystem.AddDirectory(subDir);
        _fileSystem.AddFile(file2, new MockFileData("content"));
        // Act
        var files = _fileService.ListFiles();
        // Assert
        files.Should().Contain(f => f.Name == "file1.txt");
        files.Should().Contain(f => f.Name == "subdir" || f.RelativePath.Contains("subdir"));
        files.Should().Contain(f => f.RelativePath.Contains("subdir") && f.Name == "file2.txt");
        // Verify all relative paths use forward slashes
        files.Should().OnlyContain(f => !f.RelativePath.Contains('\\'));
    }

    [Fact]
    public void SearchFiles_IgnoresDirectories_OnlySearchesFiles()
    {
        // Arrange
        var file = Path.Combine(_testProjectDirectory, "file.txt");
        var dir = Path.Combine(_testProjectDirectory, "directory");
        _fileSystem.AddFile(file, new MockFileData("search content"));
        _fileSystem.AddDirectory(dir);
        // Act
        var results = _fileService.SearchFiles("search");
        // Assert
        results.Should().Contain(f => f.RelativePath == "file.txt");
        results.Should().NotContain(f => f.RelativePath.Contains("directory"));
    }

    [Fact]
    public void SearchFiles_HandlesExceptions_ContinuesSearching()
    {
        // Arrange
        var file1 = Path.Combine(_testProjectDirectory, "file1.txt");
        var file2 = Path.Combine(_testProjectDirectory, "file2.txt");
        _fileSystem.AddFile(file1, new MockFileData("search content"));
        _fileSystem.AddFile(file2, new MockFileData("search content"));
        // Act
        var results = _fileService.SearchFiles("search");
        // Assert - Should find results from files that can be read
        results.Should().HaveCountGreaterThan(0);
        results.Should().Contain(f => f.RelativePath == "file1.txt");
        results.Should().Contain(f => f.RelativePath == "file2.txt");
    }

    [Fact]
    public void DeleteFiles_MultipleFiles_DeletesAllFiles()
    {
        // Arrange
        var files = new[]
        {
            "file1.txt",
            "file2.txt",
            "file3.txt"
        };
        foreach (var file in files)
        {
            var fullPath = Path.Combine(_testProjectDirectory, file);
            _fileSystem.AddFile(fullPath, new MockFileData("content"));
        }

        // Act
        _fileService.DeleteFiles(files);
        // Assert
        foreach (var file in files)
        {
            var fullPath = Path.Combine(_testProjectDirectory, file);
            _fileSystem.File.Exists(fullPath).Should().BeFalse();
        }
    }

    [Fact]
    public void CopyFiles_MultipleFiles_CopiesAllFiles()
    {
        // Arrange
        var operations = new[]
        {
            new FileOperation
            {
                Source = "source1.txt",
                Destination = "dest1.txt"
            },
            new FileOperation
            {
                Source = "source2.txt",
                Destination = "dest2.txt"
            },
            new FileOperation
            {
                Source = "source3.txt",
                Destination = "dest3.txt"
            }
        };
        var content = "test content";
        foreach (var op in operations)
        {
            var fullPath = Path.Combine(_testProjectDirectory, op.Source);
            _fileSystem.AddFile(fullPath, new MockFileData(content));
        }

        // Act
        _fileService.CopyFiles(operations);
        // Assert
        foreach (var op in operations)
        {
            var destFullPath = Path.Combine(_testProjectDirectory, op.Destination);
            var sourceFullPath = Path.Combine(_testProjectDirectory, op.Source);
            _fileSystem.File.Exists(destFullPath).Should().BeTrue();
            _fileSystem.File.ReadAllText(destFullPath).Should().Be(content);
            _fileSystem.File.Exists(sourceFullPath).Should().BeTrue(); // Source should still exist
        }
    }

    [Fact]
    public void MoveFiles_MultipleFiles_MovesAllFiles()
    {
        // Arrange
        var operations = new[]
        {
            new FileOperation
            {
                Source = "source1.txt",
                Destination = "dest1.txt"
            },
            new FileOperation
            {
                Source = "source2.txt",
                Destination = "dest2.txt"
            },
            new FileOperation
            {
                Source = "source3.txt",
                Destination = "dest3.txt"
            }
        };
        var content = "test content";
        foreach (var op in operations)
        {
            var fullPath = Path.Combine(_testProjectDirectory, op.Source);
            _fileSystem.AddFile(fullPath, new MockFileData(content));
        }

        // Act
        _fileService.MoveFiles(operations);
        // Assert
        foreach (var op in operations)
        {
            var destFullPath = Path.Combine(_testProjectDirectory, op.Destination);
            var sourceFullPath = Path.Combine(_testProjectDirectory, op.Source);
            _fileSystem.File.Exists(destFullPath).Should().BeTrue();
            _fileSystem.File.ReadAllText(destFullPath).Should().Be(content);
            _fileSystem.File.Exists(sourceFullPath).Should().BeFalse(); // Source should be gone
        }
    }

    [Fact]
    public void ListFiles_WithFilter_ReturnsOnlyMatchingFiles()
    {
        // Arrange
        var file1 = Path.Combine(_testProjectDirectory, "test.cs");
        var file2 = Path.Combine(_testProjectDirectory, "test.txt");
        var file3 = Path.Combine(_testProjectDirectory, "program.cs");
        _fileSystem.AddFile(file1, new MockFileData("content"));
        _fileSystem.AddFile(file2, new MockFileData("content"));
        _fileSystem.AddFile(file3, new MockFileData("content"));
        // Act
        var results = _fileService.ListFiles(".", "*.cs");
        // Assert
        results.Should().Contain(f => f.Name == "test.cs");
        results.Should().Contain(f => f.Name == "program.cs");
        results.Should().NotContain(f => f.Name == "test.txt");
    }

    [Fact]
    public void SearchFiles_WithFilter_SearchesOnlyMatchingFiles()
    {
        // Arrange
        var file1 = Path.Combine(_testProjectDirectory, "test.cs");
        var file2 = Path.Combine(_testProjectDirectory, "test.txt");
        var file3 = Path.Combine(_testProjectDirectory, "program.cs");
        _fileSystem.AddFile(file1, new MockFileData("search content"));
        _fileSystem.AddFile(file2, new MockFileData("search content"));
        _fileSystem.AddFile(file3, new MockFileData("other content"));
        // Act
        var results = _fileService.SearchFiles("search", ".", "*.cs");
        // Assert
        results.Should().Contain(f => f.RelativePath == "test.cs");
        results.Should().NotContain(f => f.RelativePath == "test.txt"); // Filtered out by pattern
        results.Should().NotContain(f => f.RelativePath == "program.cs"); // Doesn't contain search term
    }

    [Fact]
    public void ListFiles_WithMultiplePatterns_ReturnsMatchingFiles()
    {
        // Arrange
        var file1 = Path.Combine(_testProjectDirectory, "test.cs");
        var file2 = Path.Combine(_testProjectDirectory, "config.json");
        var file3 = Path.Combine(_testProjectDirectory, "readme.txt");
        _fileSystem.AddFile(file1, new MockFileData("content"));
        _fileSystem.AddFile(file2, new MockFileData("content"));
        _fileSystem.AddFile(file3, new MockFileData("content"));
        // Act
        var results = _fileService.ListFiles(".", "*.cs,*.json");
        // Assert
        results.Should().Contain(f => f.Name == "test.cs");
        results.Should().Contain(f => f.Name == "config.json");
        results.Should().NotContain(f => f.Name == "readme.txt");
    }

    [Fact]
    public void ReadFile_WithStartAndEndLine_ReturnsSpecificLines()
    {
        // Arrange
        var relativePath = "test.txt";
        var content = "Line 1\nLine 2\nLine 3\nLine 4\nLine 5";
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        _fileSystem.AddFile(fullPath, new MockFileData(content));
        // Act
        var result = _fileService.ReadFile(relativePath, 2, 4);
        // Assert
        result.Should().Contain("Line 2\nLine 3\nLine 4");
        result.Should().Contain("// ... (1 lines above)");
        result.Should().Contain("// ... (1 lines below)");
    }
}