using System.IO.Abstractions.TestingHelpers;
using CodeEditor.MCP.Services;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeEditor.MCP.Tests;

public class DocumentFormattingServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testProjectDirectory;
    private PathService _pathService = null!;
    private DocumentFormattingService _formattingService = null!;
    private MockFileSystem _fileSystem = null!;
    private FileService _fileService = null!;

    public DocumentFormattingServiceTests()
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
        _formattingService = new DocumentFormattingService(_fileService, _pathService, _fileSystem);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void FormatDocument_WithBadlyFormattedCode_FormatsCorrectly()
    {
        // Arrange
        var badlyFormattedCode = @"using System;

namespace BadFormat{
public class TestClass{
public void TestMethod(){
Console.WriteLine(""Hello World"");
}
}
}";
        var filePath = Path.Combine(_testProjectDirectory, "BadFormat.cs");
        _fileSystem.AddFile(filePath, new MockFileData(badlyFormattedCode));

        // Act
        var result = _formattingService.FormatDocument("BadFormat.cs");

        // Assert
        result.Should().Contain("Successfully formatted");
        
        // Check that the file was actually formatted
        var formattedContent = _fileSystem.File.ReadAllText(filePath);
        formattedContent.Should().NotBe(badlyFormattedCode);
        
        // Check for proper indentation and structure (more flexible)
        formattedContent.Should().Contain("namespace BadFormat");
        formattedContent.Should().Contain("public class TestClass");
        formattedContent.Should().Contain("public void TestMethod");
        formattedContent.Should().Contain("Console.WriteLine");
        
        // Verify it's properly indented (has some indentation)
        var lines = formattedContent.Split('\n');
        lines.Should().Contain(line => line.StartsWith("    ")); // Should have some indented lines
    }

    [Fact]
    public void FormatDocument_WithSyntaxErrors_ReturnsErrorMessage()
    {
        // Arrange
        var badCode = @"using System;

namespace Test{
public class BadSyntax{
public void Method1(string param1,int param2{  // Missing closing parenthesis
if(param1==null{  // Missing closing parenthesis
return;
}
Console.WriteLine(""Hello"");
}
}
}";
        var filePath = Path.Combine(_testProjectDirectory, "BadSyntax.cs");
        _fileSystem.AddFile(filePath, new MockFileData(badCode));

        // Act
        var result = _formattingService.FormatDocument("BadSyntax.cs");

        // Assert
        result.Should().Contain("Error: Cannot format document due to syntax errors");
        result.Should().Contain("Line");
    }

    [Fact]
    public void FormatDocument_FileNotFound_ReturnsErrorMessage()
    {
        // Act
        var result = _formattingService.FormatDocument("NonExistent.cs");

        // Assert
        result.Should().Contain("Error: File not found at path: NonExistent.cs");
    }

    [Fact]
    public void FormatDocument_NonCSharpFile_ReturnsErrorMessage()
    {
        // Arrange
        var filePath = Path.Combine(_testProjectDirectory, "NotCSharp.txt");
        _fileSystem.AddFile(filePath, new MockFileData("some text"));

        // Act
        var result = _formattingService.FormatDocument("NotCSharp.txt");

        // Assert
        result.Should().Contain("Error: File must be a C# source file (.cs)");
    }
    [Fact]
    public void ValidateFormatting_WithBadFormatting_ReturnsValidationFailure()
    {
        // Arrange
        var badlyFormattedCode = @"using System;

namespace Test{
public class BadFormat{
public void Method1(){
Console.WriteLine(""Hello"");
}
}
}";
        var filePath = Path.Combine(_testProjectDirectory, "BadFormatValidation.cs");
        _fileSystem.AddFile(filePath, new MockFileData(badlyFormattedCode));

        // Act
        var result = _formattingService.ValidateFormatting("BadFormatValidation.cs");

        // Assert
        result.Should().Contain("Document formatting issues found");
        result.Should().Contain("Lines with formatting differences:");
        result.Should().Contain("Run FormatDocument to fix formatting issues");
    }

    [Fact]
    public void FormatDirectory_WithMultipleFiles_FormatsAllFiles()
    {
        // Arrange
        var badCode1 = @"using System;
namespace Test{public class Class1{public void Method1(){Console.WriteLine(""Hello"");}}}";
        var badCode2 = @"using System;
namespace Test{public class Class2{public void Method2(){Console.WriteLine(""World"");}}}";
        
        var file1Path = Path.Combine(_testProjectDirectory, "Class1.cs");
        var file2Path = Path.Combine(_testProjectDirectory, "Class2.cs");
        
        _fileSystem.AddFile(file1Path, new MockFileData(badCode1));
        _fileSystem.AddFile(file2Path, new MockFileData(badCode2));

        // Act
        var result = _formattingService.FormatDirectory(".");

        // Assert
        result.Should().Contain("Formatting complete for directory:");
        result.Should().Contain("Files processed: 2");
        result.Should().Contain("Successfully formatted: 2");
        result.Should().Contain("Errors: 0");
    }
}
