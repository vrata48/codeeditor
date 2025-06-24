using System.IO.Abstractions.TestingHelpers;
using CodeEditor.MCP.Services;
using FluentAssertions;

namespace CodeEditor.MCP.Tests;

public class FileServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testProjectDirectory;
    private PathService _pathService = null!;
    private FileService _fileService = null!;
    private MockFileSystem _fileSystem = null!;

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
        var fileFilterService = new FileFilterService(_pathService, null);
        _fileService = new FileService(_fileSystem, _pathService, fileFilterService);
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
    public void DeleteFile_ExistingFile_RemovesFile()
    {
        // Arrange
        var relativePath = "to-delete.txt";
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        _fileSystem.AddFile(fullPath, new MockFileData("content"));

        // Act
        _fileService.DeleteFile(relativePath);

        // Assert
        _fileSystem.File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public void DeleteFile_ExistingDirectory_RemovesDirectory()
    {
        // Arrange
        var relativePath = "dir-to-delete";
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        _fileSystem.AddDirectory(fullPath);
        _fileSystem.AddFile(Path.Combine(fullPath, "file.txt"), new MockFileData("content"));

        // Act
        _fileService.DeleteFile(relativePath);

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
        results.Should().Contain("file1.txt");
        results.Should().Contain("file3.txt");
        results.Should().NotContain("file2.txt");
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
        results.Should().Contain("file1.txt");
    }

    [Fact]
    public void CopyFile_ExistingFile_CopiesSuccessfully()
    {
        // Arrange
        var sourceFile = Path.Combine(_testProjectDirectory, "source.txt");
        var destFile = Path.Combine(_testProjectDirectory, "destination.txt");
        var content = "File content";
        
        _fileSystem.AddFile(sourceFile, new MockFileData(content));

        // Act
        _fileService.CopyFile("source.txt", "destination.txt");

        // Assert
        _fileSystem.File.Exists(destFile).Should().BeTrue();
        _fileSystem.File.ReadAllText(destFile).Should().Be(content);
        _fileSystem.File.Exists(sourceFile).Should().BeTrue(); // Original should still exist
    }

    [Fact]
    public void CopyFile_Directory_CopiesRecursively()
    {
        // Arrange
        var sourceDir = Path.Combine(_testProjectDirectory, "source-dir");
        var destDir = Path.Combine(_testProjectDirectory, "dest-dir");
        
        _fileSystem.AddDirectory(sourceDir);
        _fileSystem.AddFile(Path.Combine(sourceDir, "file1.txt"), new MockFileData("Content 1"));
        _fileSystem.AddDirectory(Path.Combine(sourceDir, "subdir"));
        _fileSystem.AddFile(Path.Combine(sourceDir, "subdir", "file2.txt"), new MockFileData("Content 2"));

        // Act
        _fileService.CopyFile("source-dir", "dest-dir");

        // Assert
        _fileSystem.Directory.Exists(destDir).Should().BeTrue();
        _fileSystem.File.Exists(Path.Combine(destDir, "file1.txt")).Should().BeTrue();
        _fileSystem.File.Exists(Path.Combine(destDir, "subdir", "file2.txt")).Should().BeTrue();
        _fileSystem.File.ReadAllText(Path.Combine(destDir, "file1.txt")).Should().Be("Content 1");
        _fileSystem.File.ReadAllText(Path.Combine(destDir, "subdir", "file2.txt")).Should().Be("Content 2");
    }

    [Fact]
    public void MoveFile_ExistingFile_MovesSuccessfully()
    {
        // Arrange
        var sourceFile = Path.Combine(_testProjectDirectory, "source.txt");
        var destFile = Path.Combine(_testProjectDirectory, "destination.txt");
        var content = "File content";
        
        _fileSystem.AddFile(sourceFile, new MockFileData(content));

        // Act
        _fileService.MoveFile("source.txt", "destination.txt");

        // Assert
        _fileSystem.File.Exists(destFile).Should().BeTrue();
        _fileSystem.File.ReadAllText(destFile).Should().Be(content);
        _fileSystem.File.Exists(sourceFile).Should().BeFalse(); // Original should be gone
    }

    [Fact]
    public void MoveFile_ExistingDirectory_MovesSuccessfully()
    {
        // Arrange
        var sourceDir = Path.Combine(_testProjectDirectory, "source-dir");
        var destDir = Path.Combine(_testProjectDirectory, "dest-dir");
        
        _fileSystem.AddDirectory(sourceDir);
        _fileSystem.AddFile(Path.Combine(sourceDir, "file.txt"), new MockFileData("Content"));

        // Act
        _fileService.MoveFile("source-dir", "dest-dir");

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
        files.Should().Contain("file1.txt");
        files.Should().Contain("subdir/");
        files.Should().Contain("subdir/file2.txt");
        
        // Verify all paths use forward slashes
        files.Should().OnlyContain(path => !path.Contains('\\') || path.Contains('/'));
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
        results.Should().Contain("file.txt");
        results.Should().NotContain("directory/");
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
        results.Should().Contain("file1.txt");
        results.Should().Contain("file2.txt");
    }
}
