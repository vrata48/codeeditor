namespace CodeEditor.MCP.Tests;

public class GitignoreTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testProjectDirectory;
    private PathService _pathService = null!;
    private FileService _fileService = null!;
    private IFileSystem _fileSystem = null!;

    public GitignoreTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _testProjectDirectory = Path.Combine(_tempDirectory, "TestProject");
        Directory.CreateDirectory(_testProjectDirectory);
        
        // Note: PathService and FileService will be created in each test method
        // after the .gitignore file is created, since PathService reads .gitignore in constructor
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
        _fileService = new FileService(_fileSystem, _pathService);
    }

    [Fact]
    public void ListFiles_WithoutGitignore_ReturnsAllFiles()
    {
        // Arrange - Create various files WITHOUT .gitignore first
        CreateTestFile("Program.cs", "// Main program");
        CreateTestFile("app.log", "Log content");
        CreateTestFile("README.md", "# Project");
        CreateTestFile("bin/Debug/app.exe", "Binary content");
        CreateTestFile("obj/Release/app.dll", "Object file");

        // Create services AFTER files are created (no .gitignore exists)
        CreateServicesAfterGitignore();

        // Act
        var files = _fileService.ListFiles();

        // Assert - All files should be returned when no .gitignore exists
        files.Should().Contain(x => x.EndsWith("Program.cs"));
        files.Should().Contain(x => x.EndsWith("app.log"));
        files.Should().Contain(x => x.EndsWith("README.md"));
        files.Should().Contain(x => x.Contains("bin"));
        files.Should().Contain(x => x.Contains("obj"));
    }

    [Fact]
    public void ListFiles_WithBasicGitignore_FiltersCorrectly()
    {
        // Arrange - Create .gitignore FIRST
        var gitignoreContent = @"*.log
bin/
obj/";
        CreateTestFile(".gitignore", gitignoreContent);

        // Create other test files
        CreateTestFile("Program.cs", "// Main program");
        CreateTestFile("app.log", "Log content");
        CreateTestFile("debug.log", "Debug content");
        CreateTestFile("README.md", "# Project");
        CreateTestFile("bin/Debug/app.exe", "Binary content");
        CreateTestFile("obj/Release/app.dll", "Object file");

        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        // Act
        var files = _fileService.ListFiles();

        // Assert - Ignored files should not be returned
        files.Should().Contain(x => x.EndsWith("Program.cs"));
        files.Should().Contain(x => x.EndsWith("README.md"));
        files.Should().Contain(x => x.EndsWith(".gitignore"));
        files.Should().NotContain(x => x.EndsWith("app.log"));
        files.Should().NotContain(x => x.EndsWith("debug.log"));
        files.Should().NotContain(x => x.Contains("bin"));
        files.Should().NotContain(x => x.Contains("obj"));
    }

    [Fact]
    public void PathService_DirectoryPatterns_WorkWithTrailingSlash()
    {
        // Arrange - Create .gitignore with directory patterns
        var gitignoreContent = @"bin/
obj/
logs/";
        CreateTestFile(".gitignore", gitignoreContent);
        CreateServicesAfterGitignore();

        // Act & Assert - Test directory patterns with trailing slash
        _pathService.ShouldIgnore("bin/").Should().BeTrue("bin/ should match bin/ pattern");
        _pathService.ShouldIgnore("obj/").Should().BeTrue("obj/ should match obj/ pattern");
        _pathService.ShouldIgnore("logs/").Should().BeTrue("logs/ should match logs/ pattern");
        
        _pathService.ShouldIgnore("bin/debug/").Should().BeTrue("bin/debug/ should match bin/ pattern");
        _pathService.ShouldIgnore("obj/release/").Should().BeTrue("obj/release/ should match obj/ pattern");
        
        // Files should not be ignored just because they have similar names
        _pathService.ShouldIgnore("binary.txt").Should().BeFalse("binary.txt should not match bin/ pattern");
        _pathService.ShouldIgnore("objects.cs").Should().BeFalse("objects.cs should not match obj/ pattern");
    }

    [Fact]
    public void ListFiles_WithComplexGitignore_HandlesVariousPatterns()
    {
        // Arrange - Create comprehensive .gitignore FIRST
        var gitignoreContent = @"# Build results
bin/
obj/
*.dll
*.exe
*.pdb

# IDE files
.vs/
.vscode/
*.user
*.suo

# Log files
*.log
logs/

# Temporary files
temp/
*.tmp
*~

# OS files
.DS_Store
Thumbs.db

# Package files
*.nupkg
packages/

# Specific file
secret.txt";

        CreateTestFile(".gitignore", gitignoreContent);

        // Create test files that should be ignored
        CreateTestFile("app.log", "Log content");
        CreateTestFile("bin/Debug/app.exe", "Binary");
        CreateTestFile("obj/Release/app.dll", "Library");
        CreateTestFile(".vs/solution.suo", "VS settings");
        CreateTestFile(".vscode/settings.json", "VS Code settings");
        CreateTestFile("project.user", "User settings");
        CreateTestFile("logs/app.log", "Nested log");
        CreateTestFile("temp/cache.tmp", "Temp file");
        CreateTestFile("backup~", "Backup file");
        CreateTestFile(".DS_Store", "Mac file");
        CreateTestFile("Thumbs.db", "Windows file");
        CreateTestFile("package.nupkg", "Package");
        CreateTestFile("packages/lib.dll", "Package lib");
        CreateTestFile("secret.txt", "Secret data");

        // Create test files that should NOT be ignored
        CreateTestFile("Program.cs", "Main program");
        CreateTestFile("README.md", "Documentation");
        CreateTestFile("src/Models/User.cs", "User model");
        CreateTestFile("tests/UserTests.cs", "Unit tests");
        CreateTestFile("docs/api.md", "API docs");

        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        // Act
        var files = _fileService.ListFiles();

        // Assert - Only non-ignored files should be returned
        files.Should().Contain(x => x.EndsWith("Program.cs"));
        files.Should().Contain(x => x.EndsWith("README.md"));
        files.Should().Contain(x => x.EndsWith("User.cs"));
        files.Should().Contain(x => x.EndsWith("UserTests.cs"));
        files.Should().Contain(x => x.EndsWith("api.md"));
        files.Should().Contain(x => x.EndsWith(".gitignore"));

        // Verify ignored files are not present
        files.Should().NotContain(x => x.EndsWith("app.log"));
        files.Should().NotContain(x => x.EndsWith("app.exe"));
        files.Should().NotContain(x => x.EndsWith("app.dll"));
        files.Should().NotContain(x => x.Contains(".vs"));
        files.Should().NotContain(x => x.Contains(".vscode"));
        files.Should().NotContain(x => x.EndsWith("project.user"));
        files.Should().NotContain(x => x.Contains("logs"));
        files.Should().NotContain(x => x.Contains("temp"));
        files.Should().NotContain(x => x.EndsWith("backup~"));
        files.Should().NotContain(x => x.EndsWith(".DS_Store"));
        files.Should().NotContain(x => x.EndsWith("Thumbs.db"));
        files.Should().NotContain(x => x.EndsWith("package.nupkg"));
        files.Should().NotContain(x => x.Contains("packages"));
        files.Should().NotContain(x => x.EndsWith("secret.txt"));
    }

    [Fact]
    public void ListFiles_WithCommentsAndEmptyLines_IgnoresThemCorrectly()
    {
        // Arrange - Create .gitignore with comments and empty lines FIRST
        var gitignoreContent = @"# This is a comment
*.log

# Another comment
bin/

# Empty line above and below

obj/
# Final comment";

        CreateTestFile(".gitignore", gitignoreContent);
        CreateTestFile("Program.cs", "Main program");
        CreateTestFile("app.log", "Log content");
        CreateTestFile("bin/app.exe", "Binary");
        CreateTestFile("obj/app.dll", "Library");

        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        // Act
        var files = _fileService.ListFiles();

        // Assert
        files.Should().Contain(x => x.EndsWith("Program.cs"));
        files.Should().NotContain(x => x.EndsWith("app.log"));
        files.Should().NotContain(x => x.Contains("bin"));
        files.Should().NotContain(x => x.Contains("obj"));
    }

    [Fact]
    public void ListFiles_WithNestedDirectories_RespectsGitignorePatterns()
    {
        // Arrange - Create .gitignore FIRST
        var gitignoreContent = @"*.log
bin/
node_modules/";

        CreateTestFile(".gitignore", gitignoreContent);
        CreateTestFile("src/app.js", "Application code");
        CreateTestFile("src/utils/helper.js", "Helper functions");
        CreateTestFile("src/components/Button.js", "Button component");
        CreateTestFile("tests/app.test.js", "Tests");
        CreateTestFile("bin/release/app.exe", "Binary");
        CreateTestFile("node_modules/react/index.js", "React library");
        CreateTestFile("node_modules/lodash/lodash.js", "Lodash library");
        CreateTestFile("logs/app.log", "Application log");
        CreateTestFile("config/settings.json", "Configuration");

        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        // Act
        var files = _fileService.ListFiles();

        // Assert - Should include source files but exclude ignored directories
        files.Should().Contain(x => x.EndsWith("app.js"));
        files.Should().Contain(x => x.EndsWith("helper.js"));
        files.Should().Contain(x => x.EndsWith("Button.js"));
        files.Should().Contain(x => x.EndsWith("app.test.js"));
        files.Should().Contain(x => x.EndsWith("settings.json"));

        files.Should().NotContain(x => x.Contains("bin"));
        files.Should().NotContain(x => x.Contains("node_modules"));
        files.Should().NotContain(x => x.EndsWith("app.log"));
    }

    [Fact]
    public void ListFiles_WithGlobPatterns_HandlesCorrectly()
    {
        // Arrange - Create .gitignore with glob patterns FIRST
        var gitignoreContent = @"*.log
test-*
**/temp/
build/*/
!important.log";

        CreateTestFile(".gitignore", gitignoreContent);
        CreateTestFile("app.log", "Log content");
        CreateTestFile("important.log", "Important log - should not be ignored due to !");
        CreateTestFile("test-file.txt", "Test file");
        CreateTestFile("test-data.json", "Test data");
        CreateTestFile("src/temp/cache.txt", "Nested temp");
        CreateTestFile("build/debug/app.exe", "Debug build");
        CreateTestFile("build/release/app.exe", "Release build");
        CreateTestFile("regular-file.txt", "Regular file");

        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        // Act
        var files = _fileService.ListFiles();

        // Assert
        files.Should().Contain(x => x.EndsWith("regular-file.txt"));
        files.Should().Contain(x => x.EndsWith("important.log")); // Should NOT be ignored due to !
        
        files.Should().NotContain(x => x.EndsWith("app.log"));
        files.Should().NotContain(x => x.EndsWith("test-file.txt"));
        files.Should().NotContain(x => x.EndsWith("test-data.json"));
        files.Should().NotContain(x => x.Contains("temp"));
        files.Should().NotContain(x => x.Contains("build"));
    }

    [Fact]
    public void ListFiles_WithMalformedGitignore_DoesNotCrash()
    {
        // Arrange - Create malformed .gitignore FIRST
        var gitignoreContent = @"*.log
[invalid regex
***/broken/pattern
# This should work
bin/";

        CreateTestFile(".gitignore", gitignoreContent);
        CreateTestFile("app.log", "Log content");
        CreateTestFile("bin/app.exe", "Binary");
        CreateTestFile("normal.txt", "Normal file");

        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        // Act & Assert - Should not throw
        var files = _fileService.ListFiles();
        
        // The behavior may vary depending on how the Ignore library handles malformed patterns,
        // but it should not crash
        files.Should().NotBeNull();
        files.Should().Contain(x => x.EndsWith("normal.txt"));
    }

    [Fact]
    public void ListFiles_GitignoreInSubdirectory_DoesNotAffectRootListing()
    {
        // Arrange - Create .gitignore in subdirectory (should be ignored)
        CreateTestFile("src/.gitignore", "*.cs"); // This should be ignored as it's not in root
        CreateTestFile("Program.cs", "Main program");
        CreateTestFile("src/Helper.cs", "Helper class");

        // Create services (no .gitignore in root)
        CreateServicesAfterGitignore();

        // Act
        var files = _fileService.ListFiles();

        // Assert - Subdirectory .gitignore should not affect root listing
        files.Should().Contain(x => x.EndsWith("Program.cs"));
        files.Should().Contain(x => x.EndsWith("Helper.cs"));
        files.Should().Contain(x => x.EndsWith(".gitignore")); // The subdirectory .gitignore file itself
    }

    [Fact]
    public void PathService_ShouldIgnore_WorksWithIndividualPaths()
    {
        // Arrange - Create .gitignore FIRST
        var gitignoreContent = @"*.log
bin/
.vs/";
        CreateTestFile(".gitignore", gitignoreContent);

        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        // Act & Assert - Test individual path checking
        _pathService.ShouldIgnore("app.log").Should().BeTrue();
        _pathService.ShouldIgnore("debug.log").Should().BeTrue();
        _pathService.ShouldIgnore("bin/").Should().BeTrue();
        _pathService.ShouldIgnore("bin/app.exe").Should().BeTrue();
        _pathService.ShouldIgnore("bin/debug/").Should().BeTrue();
        _pathService.ShouldIgnore("bin/debug/app.dll").Should().BeTrue();
        _pathService.ShouldIgnore(".vs/").Should().BeTrue();
        _pathService.ShouldIgnore(".vs/solution.suo").Should().BeTrue();
        
        _pathService.ShouldIgnore("Program.cs").Should().BeFalse();
        _pathService.ShouldIgnore("README.md").Should().BeFalse();
        _pathService.ShouldIgnore("src/Helper.cs").Should().BeFalse();
    }

    [Fact]
    public void PathService_FilterIgnored_RemovesIgnoredPaths()
    {
        // Arrange - Create .gitignore FIRST
        var gitignoreContent = @"*.log
bin/";
        CreateTestFile(".gitignore", gitignoreContent);

        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        var allPaths = new[]
        {
            "Program.cs",
            "app.log",
            "bin/",
            "bin/app.exe",
            "README.md",
            "debug.log"
        };

        // Act
        var filteredPaths = _pathService.FilterIgnored(allPaths).ToArray();

        // Assert
        filteredPaths.Should().Contain("Program.cs");
        filteredPaths.Should().Contain("README.md");
        filteredPaths.Should().NotContain("app.log");
        filteredPaths.Should().NotContain("bin/");
        filteredPaths.Should().NotContain("bin/app.exe");
        filteredPaths.Should().NotContain("debug.log");
    }

    [Fact]
    public void PathService_WithGitignore_AutomaticallyIgnoresGitDirectory()
    {
        // Arrange - Create .gitignore file (even with simple content)
        var gitignoreContent = @"*.log
bin/";
        CreateTestFile(".gitignore", gitignoreContent);
        
        // Create .git directory and files
        CreateTestFile(".git/config", "Git config");
        CreateTestFile(".git/HEAD", "ref: refs/heads/main");
        CreateTestFile(".git/objects/abc123", "Git object");
        CreateTestFile(".git/refs/heads/main", "commit hash");
        
        // Create regular files
        CreateTestFile("Program.cs", "Main program");
        CreateTestFile("README.md", "Documentation");
        
        // Create services AFTER .gitignore is created
        CreateServicesAfterGitignore();

        // Act
        var files = _fileService.ListFiles();

        // Assert - .git directory should be automatically ignored
        files.Should().Contain(x => x.EndsWith("Program.cs"));
        files.Should().Contain(x => x.EndsWith("README.md"));
        files.Should().Contain(x => x.EndsWith(".gitignore"));
        
        // .git directory and its contents should be ignored
        files.Should().NotContain(x => x.Contains(".git"));
        
        // Verify directly with PathService
        _pathService.ShouldIgnore(".git/").Should().BeTrue(".git/ should be automatically ignored");
        _pathService.ShouldIgnore(".git/config").Should().BeTrue(".git/config should be ignored");
        _pathService.ShouldIgnore(".git/objects/abc123").Should().BeTrue("nested .git files should be ignored");
    }

    private void CreateTestFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_testProjectDirectory, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }
        
        File.WriteAllText(fullPath, content);
    }
}
