using CodeEditor.MCP.Services;
using FluentAssertions;

namespace CodeEditor.MCP.Tests;

public class PathServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testProjectDirectory;

    public PathServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _testProjectDirectory = Path.Combine(_tempDirectory, "TestProject");
        Directory.CreateDirectory(_testProjectDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void Constructor_WithValidDirectory_SetsBaseDirectory()
    {
        // Act
        var pathService = new PathService(_testProjectDirectory);

        // Assert
        pathService.GetBaseDirectory().Should().Be(_testProjectDirectory);
    }

    [Fact]
    public void GetFullPath_WithRelativePath_ReturnsFullPath()
    {
        // Arrange
        var pathService = new PathService(_testProjectDirectory);
        var relativePath = "test\file.txt";

        // Act
        var fullPath = pathService.GetFullPath(relativePath);

        // Assert
        var expectedPath = Path.Combine(_testProjectDirectory, relativePath);
        fullPath.Should().Be(expectedPath);
    }

    [Fact]
    public void GetFullPath_WithEmptyPath_ReturnsBaseDirectory()
    {
        // Arrange
        var pathService = new PathService(_testProjectDirectory);

        // Act
        var fullPath = pathService.GetFullPath("");

        // Assert
        fullPath.Should().Be(_testProjectDirectory);
    }

    [Fact]
    public void GetFullPath_WithDotPath_ReturnsBaseDirectory()
    {
        // Arrange
        var pathService = new PathService(_testProjectDirectory);

        // Act
        var fullPath = pathService.GetFullPath(".");

        // Assert
        fullPath.Should().Be(_testProjectDirectory);
    }

    [Fact]
    public void ShouldIgnore_WithoutGitignore_ReturnsFalse()
    {
        // Arrange
        var pathService = new PathService(_testProjectDirectory);

        // Act & Assert
        pathService.ShouldIgnore("any-file.txt").Should().BeFalse();
        pathService.ShouldIgnore("bin/").Should().BeFalse();
        pathService.ShouldIgnore("*.log").Should().BeFalse();
    }

    [Fact]
    public void ShouldIgnore_WithGitignore_RespectsPatterns()
    {
        // Arrange
        var gitignoreContent = @"*.log
bin/
obj/
temp.txt";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        var pathService = new PathService(_testProjectDirectory);

        // Act & Assert
        pathService.ShouldIgnore("app.log").Should().BeTrue();
        pathService.ShouldIgnore("debug.log").Should().BeTrue();
        pathService.ShouldIgnore("bin/").Should().BeTrue();
        pathService.ShouldIgnore("bin/debug/").Should().BeTrue();
        pathService.ShouldIgnore("obj/").Should().BeTrue();
        pathService.ShouldIgnore("temp.txt").Should().BeTrue();
        
        pathService.ShouldIgnore("app.txt").Should().BeFalse();
        pathService.ShouldIgnore("source.cs").Should().BeFalse();
        pathService.ShouldIgnore("readme.md").Should().BeFalse();
    }

    [Fact]
    public void FilterIgnored_WithoutGitignore_ReturnsAllPaths()
    {
        // Arrange
        var pathService = new PathService(_testProjectDirectory);
        var paths = new[] { "file1.txt", "file2.log", "bin/app.exe", "src/code.cs" };

        // Act
        var filtered = pathService.FilterIgnored(paths);

        // Assert
        filtered.Should().BeEquivalentTo(paths);
    }

    [Fact]
    public void FilterIgnored_WithGitignore_FiltersCorrectly()
    {
        // Arrange
        var gitignoreContent = @"*.log
bin/";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        var pathService = new PathService(_testProjectDirectory);
        var paths = new[] { "file1.txt", "file2.log", "bin/app.exe", "src/code.cs", "debug.log" };

        // Act
        var filtered = pathService.FilterIgnored(paths).ToArray();

        // Assert
        filtered.Should().Contain("file1.txt");
        filtered.Should().Contain("src/code.cs");
        filtered.Should().NotContain("file2.log");
        filtered.Should().NotContain("bin/app.exe");
        filtered.Should().NotContain("debug.log");
    }

    [Fact]
    public void GitignoreHandling_WithComments_IgnoresCommentLines()
    {
        // Arrange
        var gitignoreContent = @"# This is a comment
*.log
# Another comment
bin/
    # Indented comment
obj/";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        var pathService = new PathService(_testProjectDirectory);

        // Act & Assert
        pathService.ShouldIgnore("app.log").Should().BeTrue();
        pathService.ShouldIgnore("bin/").Should().BeTrue();
        pathService.ShouldIgnore("obj/").Should().BeTrue();
    }

    [Fact]
    public void GitignoreHandling_WithEmptyLines_IgnoresEmptyLines()
    {
        // Arrange
        var gitignoreContent = @"*.log

bin/


obj/

";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        var pathService = new PathService(_testProjectDirectory);

        // Act & Assert
        pathService.ShouldIgnore("app.log").Should().BeTrue();
        pathService.ShouldIgnore("bin/").Should().BeTrue();
        pathService.ShouldIgnore("obj/").Should().BeTrue();
    }

    [Fact]
    public void GitignoreHandling_WithNegationPatterns_HandlesCorrectly()
    {
        // Arrange
        var gitignoreContent = @"*.log
!important.log
bin/
!bin/keep.txt";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        var pathService = new PathService(_testProjectDirectory);

        // Act & Assert
        pathService.ShouldIgnore("app.log").Should().BeTrue();
        pathService.ShouldIgnore("important.log").Should().BeFalse(); // Should not be ignored due to !
        pathService.ShouldIgnore("bin/app.exe").Should().BeTrue();
        pathService.ShouldIgnore("bin/keep.txt").Should().BeFalse(); // Should not be ignored due to !
    }

    [Fact]
    public void GitignoreHandling_WithDirectoryPatterns_MatchesDirectoriesAndContents()
    {
        // Arrange
        var gitignoreContent = @"bin/
logs/";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        var pathService = new PathService(_testProjectDirectory);

        // Act & Assert
        pathService.ShouldIgnore("bin/").Should().BeTrue();
        pathService.ShouldIgnore("bin/debug/").Should().BeTrue();
        pathService.ShouldIgnore("bin/debug/app.exe").Should().BeTrue();
        pathService.ShouldIgnore("logs/").Should().BeTrue();
        pathService.ShouldIgnore("logs/app.log").Should().BeTrue();
        
        // Files with similar names should not be ignored
        pathService.ShouldIgnore("binary.txt").Should().BeFalse();
        pathService.ShouldIgnore("logger.cs").Should().BeFalse();
    }

    [Fact]
    public void GitignoreHandling_WithGlobPatterns_HandlesWildcards()
    {
        // Arrange
        var gitignoreContent = @"test-*
**/temp/
*.tmp";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        var pathService = new PathService(_testProjectDirectory);

        // Act & Assert
        pathService.ShouldIgnore("test-file.txt").Should().BeTrue();
        pathService.ShouldIgnore("test-data.json").Should().BeTrue();
        pathService.ShouldIgnore("src/temp/").Should().BeTrue();
        pathService.ShouldIgnore("build/temp/cache.txt").Should().BeTrue();
        pathService.ShouldIgnore("cache.tmp").Should().BeTrue();
        
        pathService.ShouldIgnore("my-test.txt").Should().BeFalse();
        pathService.ShouldIgnore("temperature.txt").Should().BeFalse();
        pathService.ShouldIgnore("template.txt").Should().BeFalse();
    }

    [Fact]
    public void GitignoreHandling_WithMalformedPatterns_DoesNotCrash()
    {
        // Arrange
        var gitignoreContent = @"*.log
[invalid regex pattern
***/broken
# This should work
bin/";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        // Act & Assert - Should not throw exception
        var pathService = new PathService(_testProjectDirectory);
        
        // Valid patterns should still work
        pathService.ShouldIgnore("app.log").Should().BeTrue();
        pathService.ShouldIgnore("bin/").Should().BeTrue();
    }

    [Fact]
    public void GitignoreHandling_WithCaseSensitivity_HandlesCorrectly()
    {
        // Arrange
        var gitignoreContent = @"*.LOG
BIN/";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        var pathService = new PathService(_testProjectDirectory);

        // Act & Assert
        // The behavior depends on the underlying Ignore library and OS
        // We just ensure it doesn't crash and behaves consistently
        var shouldIgnoreLog = pathService.ShouldIgnore("app.log");
        var shouldIgnoreLOG = pathService.ShouldIgnore("app.LOG");
        var shouldIgnoreBin = pathService.ShouldIgnore("bin/");
        var shouldIgnoreBIN = pathService.ShouldIgnore("BIN/");

        // At least one case should work
        (shouldIgnoreLog || shouldIgnoreLOG).Should().BeTrue();
        (shouldIgnoreBin || shouldIgnoreBIN).Should().BeTrue();
    }

    [Theory]
    [InlineData("file.txt")]
    [InlineData("./file.txt")]
    [InlineData("sub/file.txt")]
    [InlineData("sub/../file.txt")]
    public void GetFullPath_WithVariousRelativePaths_ResolvesCorrectly(string relativePath)
    {
        // Arrange
        var pathService = new PathService(_testProjectDirectory);

        // Act
        var fullPath = pathService.GetFullPath(relativePath);

        // Assert
        fullPath.Should().StartWith(_testProjectDirectory);
        fullPath.Should().NotContain("..");
        Path.IsPathRooted(fullPath).Should().BeTrue();
    }

    [Fact]
    public void FilterIgnored_WithLargeNumberOfPaths_PerformsReasonably()
    {
        // Arrange
        var gitignoreContent = @"*.log
bin/
obj/
temp/";
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, gitignoreContent);

        var pathService = new PathService(_testProjectDirectory);
        
        // Create a large number of paths to test performance
        var paths = new List<string>();
        for (int i = 0; i < 1000; i++)
        {
            paths.Add($"file{i}.txt");
            paths.Add($"app{i}.log");
            paths.Add($"bin/file{i}.exe");
            paths.Add($"src/class{i}.cs");
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var filtered = pathService.FilterIgnored(paths).ToList();
        stopwatch.Stop();

        // Assert
        filtered.Should().HaveCountLessThan(paths.Count); // Some should be filtered
        filtered.Should().NotContain(p => p.EndsWith(".log"));
        filtered.Should().NotContain(p => p.StartsWith("bin/"));
        
        // Performance should be reasonable (adjust threshold as needed)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 5 seconds
    }

    [Fact]
    public void GitignoreReload_WhenFileChanges_UpdatesPatterns()
    {
        // Arrange
        var gitignorePath = Path.Combine(_testProjectDirectory, ".gitignore");
        File.WriteAllText(gitignorePath, "*.log");

        var pathService = new PathService(_testProjectDirectory);
        
        // Verify initial state
        pathService.ShouldIgnore("app.log").Should().BeTrue();
        pathService.ShouldIgnore("app.txt").Should().BeFalse();

        // Update gitignore content
        File.WriteAllText(gitignorePath, "*.txt");
        
        // Create new PathService instance (simulating reload)
        var updatedPathService = new PathService(_testProjectDirectory);

        // Act & Assert
        updatedPathService.ShouldIgnore("app.log").Should().BeFalse();
        updatedPathService.ShouldIgnore("app.txt").Should().BeTrue();
    }
}
