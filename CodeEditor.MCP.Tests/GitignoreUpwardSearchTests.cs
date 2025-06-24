using System.IO.Abstractions;
using CodeEditor.MCP.Services;
using FluentAssertions;

namespace CodeEditor.MCP.Tests;

public class GitignoreUpwardSearchTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _rootProjectDirectory;
    private readonly string _subDirectory;
    private readonly string _deepSubDirectory;
    private PathService _pathService = null!;
    private FileService _fileService = null!;
    private IFileSystem _fileSystem = null!;

    public GitignoreUpwardSearchTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _rootProjectDirectory = Path.Combine(_tempDirectory, "RootProject");
        _subDirectory = Path.Combine(_rootProjectDirectory, "src", "components");
        _deepSubDirectory = Path.Combine(_subDirectory, "ui", "forms");
        
        Directory.CreateDirectory(_deepSubDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
private void CreateServicesFromDirectory(string workingDirectory)
    {
        _pathService = new PathService(workingDirectory);
        _fileSystem = new System.IO.Abstractions.FileSystem();
        var fileFilterService = new FileFilterService(_pathService, null);
        _fileService = new FileService(_fileSystem, _pathService, fileFilterService);
    } 
    private void CreateTestFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_rootProjectDirectory, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }
        
        File.WriteAllText(fullPath, content);
    }

    [Fact]
    public void PathService_SearchesUpwardForGitignoreFiles()
    {
        // Arrange - Create .gitignore files at different levels
        
        // Root level .gitignore
        var rootGitignoreContent = @"*.log
bin/
obj/";
        File.WriteAllText(Path.Combine(_rootProjectDirectory, ".gitignore"), rootGitignoreContent);
        
        // Parent directory .gitignore (one level up from temp)
        var parentGitignoreContent = @"*.tmp
temp/";
        File.WriteAllText(Path.Combine(_tempDirectory, ".gitignore"), parentGitignoreContent);
        
        // Create test files
        CreateTestFile("Program.cs", "// Main program");
        CreateTestFile("app.log", "Log content");
        CreateTestFile("temp.tmp", "Temp content");
        CreateTestFile("bin/app.exe", "Binary");
        CreateTestFile("src/components/Button.cs", "Button component");
        
        // Create services from a subdirectory - it should find gitignore files upward
        CreateServicesFromDirectory(_subDirectory);

        // Act & Assert - Test that patterns from both gitignore files are applied
        _pathService.ShouldIgnore("app.log").Should().BeTrue("*.log pattern should be found from root .gitignore");
        _pathService.ShouldIgnore("debug.log").Should().BeTrue("*.log pattern should match any .log file");
        _pathService.ShouldIgnore("bin/").Should().BeTrue("bin/ pattern should be found from root .gitignore");
        _pathService.ShouldIgnore("temp.tmp").Should().BeTrue("*.tmp pattern should be found from parent .gitignore");
        _pathService.ShouldIgnore("temp/").Should().BeTrue("temp/ pattern should be found from parent .gitignore");
        
        _pathService.ShouldIgnore("Program.cs").Should().BeFalse("CS files should not be ignored");
        _pathService.ShouldIgnore("Button.cs").Should().BeFalse("CS files should not be ignored");
    }

    [Fact]
    public void PathService_WorksWhenNoGitignoreFound()
    {
        // Arrange - Create services without any .gitignore files
        CreateServicesFromDirectory(_subDirectory);

        // Act & Assert - Should work without crashing
        _pathService.ShouldIgnore("app.log").Should().BeFalse("No patterns should match when no .gitignore exists");
        _pathService.ShouldIgnore("bin/app.exe").Should().BeFalse("No patterns should match when no .gitignore exists");
        
        // .git should still be ignored by default
        _pathService.ShouldIgnore(".git/").Should().BeTrue(".git should always be ignored");
        _pathService.ShouldIgnore(".git/config").Should().BeTrue(".git contents should always be ignored");
    }

    [Fact]
    public void PathService_HandlesMultipleGitignoreFiles()
    {
        // Arrange - Create a hierarchy of .gitignore files
        
        // Grandparent .gitignore
        var grandparentGitignoreContent = @"*.bak
cache/";
        File.WriteAllText(Path.Combine(_tempDirectory, ".gitignore"), grandparentGitignoreContent);
        
        // Parent .gitignore (root project)
        var parentGitignoreContent = @"*.log
bin/
obj/";
        File.WriteAllText(Path.Combine(_rootProjectDirectory, ".gitignore"), parentGitignoreContent);
        
        // Intermediate .gitignore
        var intermediateGitignoreContent = @"*.tmp
node_modules/";
        File.WriteAllText(Path.Combine(_rootProjectDirectory, "src", ".gitignore"), intermediateGitignoreContent);
        
        // Create services from deep subdirectory
        CreateServicesFromDirectory(_deepSubDirectory);

        // Act & Assert - All patterns from all levels should be active
        _pathService.ShouldIgnore("file.bak").Should().BeTrue("*.bak from grandparent should work");
        _pathService.ShouldIgnore("cache/").Should().BeTrue("cache/ from grandparent should work");
        _pathService.ShouldIgnore("app.log").Should().BeTrue("*.log from parent should work");
        _pathService.ShouldIgnore("bin/").Should().BeTrue("bin/ from parent should work");
        _pathService.ShouldIgnore("obj/").Should().BeTrue("obj/ from parent should work");
        _pathService.ShouldIgnore("temp.tmp").Should().BeTrue("*.tmp from intermediate should work");
        _pathService.ShouldIgnore("node_modules/").Should().BeTrue("node_modules/ from intermediate should work");
        
        _pathService.ShouldIgnore("Program.cs").Should().BeFalse("CS files should not be ignored");
    }
[Fact]
    public void FileService_ListFiles_RespectsUpwardGitignoreFiles()
    {
        // Arrange - Create .gitignore in parent directory
        var parentGitignoreContent = @"*.log
build/
*.tmp";
        File.WriteAllText(Path.Combine(_tempDirectory, ".gitignore"), parentGitignoreContent);
        
        // Create root project .gitignore  
        var rootGitignoreContent = @"bin/
obj/";
        File.WriteAllText(Path.Combine(_rootProjectDirectory, ".gitignore"), rootGitignoreContent);
        
        // Create test files in the subdirectory where we'll be working
        CreateTestFile("src/components/Button.cs", "Button component");
        CreateTestFile("src/components/app.log", "Log content");
        CreateTestFile("src/components/cache.tmp", "Temp content");
        CreateTestFile("src/components/ui/forms/LoginForm.cs", "Login form");
        CreateTestFile("src/components/ui/forms/debug.log", "Debug log");
        
        // Create services from subdirectory
        CreateServicesFromDirectory(_subDirectory);

        // Act
        var files = _fileService.ListFiles();

        // Assert - Files matching patterns from any level should be filtered out
        files.Should().Contain(x => x.EndsWith("Button.cs"));
        files.Should().Contain(x => x.EndsWith("LoginForm.cs"));
        
        files.Should().NotContain(x => x.EndsWith("app.log")); // Filtered by parent .gitignore
        files.Should().NotContain(x => x.EndsWith("cache.tmp")); // Filtered by parent .gitignore
        files.Should().NotContain(x => x.EndsWith("debug.log")); // Filtered by parent .gitignore
    } 
    [Fact]
    public void PathService_StopsAtFileSystemRoot()
    {
        // Arrange - This test ensures we don't infinite loop trying to go above filesystem root
        // We'll use a path close to root to test this behavior
        var nearRootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        if (string.IsNullOrEmpty(nearRootPath))
        {
            // Skip test if we can't determine root
            return;
        }

        // Act & Assert - Should not throw or hang
        var pathService = new PathService(nearRootPath);
        pathService.ShouldIgnore("test.txt").Should().BeFalse("Should handle paths near filesystem root");
    }

    [Fact]
    public void PathService_HandlesRelativePathsInWorkingDirectory()
    {
        // Arrange - Create .gitignore in parent
        var parentGitignoreContent = @"*.log
temp/";
        File.WriteAllText(Path.Combine(_rootProjectDirectory, ".gitignore"), parentGitignoreContent);
        
        CreateTestFile("src/components/app.log", "Log in subdir");
        CreateTestFile("src/components/Component.cs", "Component");
        
        // Create services from subdirectory
        CreateServicesFromDirectory(_subDirectory);

        // Act & Assert - Relative paths should work correctly
        _pathService.ShouldIgnore("app.log").Should().BeTrue("Local .log file should be ignored");
        _pathService.ShouldIgnore("Component.cs").Should().BeFalse("CS file should not be ignored");
        
        // Test with relative paths that go up
        _pathService.ShouldIgnore("../app.log").Should().BeTrue("Parent .log file should be ignored");
        _pathService.ShouldIgnore("../../app.log").Should().BeTrue("Grandparent .log file should be ignored");
    }
}
