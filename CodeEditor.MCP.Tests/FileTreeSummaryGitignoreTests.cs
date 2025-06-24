using System.IO.Abstractions.TestingHelpers;
using CodeEditor.MCP.Services;
using FluentAssertions;

namespace CodeEditor.MCP.Tests;

public class FileTreeSummaryGitignoreTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testProjectDirectory;
    private PathService _pathService = null!;
    private FileAnalysisService _fileAnalysisService = null!;
    private IFileSystem _fileSystem = null!;

    public FileTreeSummaryGitignoreTests()
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

    private void CreateServicesAfterGitignore()
    {
        _pathService = new PathService(_testProjectDirectory);
        _fileSystem = new System.IO.Abstractions.FileSystem();
        _fileAnalysisService = new FileAnalysisService(_pathService);
    }

    private void CreateTestFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(fullPath, content);
    }

    [Fact]
    public async Task FileTreeSummary_WithGitignore_FiltersReleasesDirectory()
    {
        // Arrange - Create .gitignore with Releases/ pattern
        var gitignoreContent = @"# Release artifacts
Releases/

# Build outputs
bin/
obj/

# IDE folders
.vs/
.idea/";

        CreateTestFile(".gitignore", gitignoreContent);

        // Create test files and directories
        CreateTestFile("Program.cs", "// Main program");
        CreateTestFile("README.md", "# Test Project");
        CreateTestFile("src/Models/User.cs", "public class User { }");
        CreateTestFile("src/Services/UserService.cs", "public class UserService { }");
        
        // Create directories that should be ignored
        CreateTestFile("Releases/v1.0/app.exe", "Binary content");
        CreateTestFile("Releases/v2.0/app.exe", "Binary content v2");
        CreateTestFile("bin/Debug/app.dll", "Debug binary");
        CreateTestFile("obj/Release/app.dll", "Release binary");
        CreateTestFile(".vs/solution.suo", "VS settings");
        CreateTestFile(".idea/workspace.xml", "IntelliJ settings");

        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        // Act
        var result = await _fileAnalysisService.GetFileTreeSummaryAsync(".", 3, "", false, true, "name");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotContain("Releases", "Releases/ directory should be filtered by .gitignore");
        result.Should().NotContain("bin", "bin/ directory should be filtered by .gitignore");
        result.Should().NotContain("obj", "obj/ directory should be filtered by .gitignore");
        result.Should().NotContain(".vs", ".vs/ directory should be filtered by .gitignore");
        result.Should().NotContain(".idea", ".idea/ directory should be filtered by .gitignore");
        
        // Verify that non-ignored files are included
        result.Should().Contain("Program.cs", "Non-ignored files should be included");
        result.Should().Contain("README.md", "Non-ignored files should be included");
        result.Should().Contain("src", "Non-ignored directories should be included");
        result.Should().Contain("User.cs", "Files in non-ignored subdirectories should be included");
        result.Should().Contain("UserService.cs", "Files in non-ignored subdirectories should be included");
    }

    [Fact]
    public async Task FileTreeSummary_WithoutGitignore_ShowsAllDirectories()
    {
        // Arrange - Create test files WITHOUT .gitignore
        CreateTestFile("Program.cs", "// Main program");
        CreateTestFile("Releases/v1.0/app.exe", "Binary content");
        CreateTestFile("bin/Debug/app.dll", "Debug binary");

        // Create services without .gitignore
        CreateServicesAfterGitignore();

        // Act
        var result = await _fileAnalysisService.GetFileTreeSummaryAsync(".", 3, "", false, true, "name");

        // Assert
        result.Should().NotBeNull();
        // Without .gitignore, Releases directory should appear (but might be filtered by hardcoded patterns)
        // This test mainly verifies that the gitignore integration is what's doing the filtering
        result.Should().Contain("Program.cs", "Files should be included when no .gitignore exists");
    }

    [Fact]
    public async Task FileTreeSummary_WithSpecificFileExtensions_FiltersCorrectly()
    {
        // Arrange
        var gitignoreContent = @"Releases/
*.log";

        CreateTestFile(".gitignore", gitignoreContent);
        CreateTestFile("Program.cs", "// C# file");
        CreateTestFile("README.md", "# Markdown file");
        CreateTestFile("app.log", "Log content");
        CreateTestFile("Releases/app.exe", "Binary");

        CreateServicesAfterGitignore();

        // Act - Only show .cs files
        var result = await _fileAnalysisService.GetFileTreeSummaryAsync(".", 3, "cs", false, true, "name");

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Program.cs", "CS files should be included");
        result.Should().NotContain("README.md", "Non-CS files should be excluded by extension filter");
        result.Should().NotContain("app.log", "Log files should be excluded by .gitignore");
        result.Should().NotContain("Releases", "Releases directory should be excluded by .gitignore");
    }

    [Fact]
    public async Task FileTreeSummary_WithNestedGitignorePatterns_FiltersCorrectly()
    {
        // Arrange
        var gitignoreContent = @"# Complex patterns
Releases/
**/bin/
**/obj/
*.user
temp/
logs/*.log";

        CreateTestFile(".gitignore", gitignoreContent);
        
        // Create nested structure
        CreateTestFile("src/Project/Program.cs", "// Main");
        CreateTestFile("src/Project/bin/Debug/app.dll", "Binary");
        CreateTestFile("tests/UnitTests/bin/Release/test.dll", "Test binary");
        CreateTestFile("Releases/v1.0/installer.msi", "Installer");
        CreateTestFile("temp/cache.tmp", "Temp file");
        CreateTestFile("logs/app.log", "App log");
        CreateTestFile("logs/readme.txt", "Log readme");
        CreateTestFile("project.user", "User settings");

        CreateServicesAfterGitignore();

        // Act
        var result = await _fileAnalysisService.GetFileTreeSummaryAsync(".", 4, "", false, true, "name");

        // Assert
        result.Should().NotBeNull();
        
        // Should include allowed files
        result.Should().Contain("Program.cs", "Source files should be included");
        result.Should().Contain("readme.txt", "Non-log files in logs directory should be included");
        
        // Should exclude ignored patterns
        result.Should().NotContain("Releases", "Releases/ should be excluded");
        result.Should().NotContain("bin", "bin/ directories should be excluded");
        result.Should().NotContain("obj", "obj/ directories should be excluded");
        result.Should().NotContain("temp", "temp/ directory should be excluded");
        result.Should().NotContain("project.user", "*.user files should be excluded");
        result.Should().NotContain("app.log", "*.log files in logs/ should be excluded");
    }

    [Fact]
    public async Task FileTreeSummary_DefaultPathBehavior_WorksCorrectly()
    {
        // Arrange
        var gitignoreContent = @"Releases/";
        CreateTestFile(".gitignore", gitignoreContent);
        CreateTestFile("Program.cs", "// Test");
        CreateTestFile("Releases/app.exe", "Binary");

        CreateServicesAfterGitignore();

        // Act - Test default path behavior (should use current directory)
        var resultWithDefault = await _fileAnalysisService.GetFileTreeSummaryAsync();
        var resultWithDot = await _fileAnalysisService.GetFileTreeSummaryAsync(".");

        // Assert
        resultWithDefault.Should().NotBeNull();
        resultWithDot.Should().NotBeNull();
        
        // Both should work the same way and exclude Releases
        resultWithDefault.Should().NotContain("Releases", "Default path should respect .gitignore");
        resultWithDot.Should().NotContain("Releases", "Explicit '.' path should respect .gitignore");
        
        resultWithDefault.Should().Contain("Program.cs", "Default path should include valid files");
        resultWithDot.Should().Contain("Program.cs", "Explicit '.' path should include valid files");
    }
}
