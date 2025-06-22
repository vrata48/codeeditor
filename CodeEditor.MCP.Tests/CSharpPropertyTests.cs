using System.IO.Abstractions.TestingHelpers;
using CodeEditor.MCP.Services;
using FluentAssertions;
using Xunit;

namespace CodeEditor.MCP.Tests;

public class CSharpPropertyTests
{
[Fact]
    public void AddProperty_ShouldAddPropertyToClass()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var pathService = new PathService("C:\\TestProject");
        var service = new CSharpService(fileSystem, pathService);
        
        var testCode = @"using System;

namespace TestApp
{
    public class TestClass
    {
        public void SomeMethod()
        {
            Console.WriteLine(""Hello World"");
        }
    }
}";
        fileSystem.AddFile("C:\\TestProject\\TestFile.cs", testCode);
        
        // Act
        service.AddProperty("TestFile.cs", "TestClass", "public string Name { get; set; }");
        
        // Assert
        var result = fileSystem.GetFile("C:\\TestProject\\TestFile.cs").TextContents;
        result.Should().Contain("public string Name { get; set; }");
    }     
[Fact]
    public void AnalyzeFile_ShouldListPropertiesAndMethods()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var pathService = new PathService("C:\\TestProject");
        var service = new CSharpService(fileSystem, pathService);
        
        var testCode = @"using System;

namespace TestApp
{
    public class TestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        
        public void SomeMethod()
        {
            Console.WriteLine(""Hello World"");
        }
    }
}";
        fileSystem.AddFile("C:\\TestProject\\TestFile.cs", testCode);
        
        // Act
        var result = service.AnalyzeFile("TestFile.cs");
        
        // Assert
        result.Should().Contain("\"name\": \"TestClass\"");
        result.Should().Contain("\"name\": \"Name\"");
        result.Should().Contain("\"name\": \"Age\"");
        result.Should().Contain("\"name\": \"SomeMethod\"");
    } [Fact]
    public void RemoveProperty_ShouldRemovePropertyFromClass()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var pathService = new PathService("C:\\TestProject");
        var service = new CSharpService(fileSystem, pathService);
        
        var testCode = @"using System;

namespace TestApp
{
    public class TestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        
        public void SomeMethod()
        {
            Console.WriteLine(""Hello World"");
        }
    }
}";
        fileSystem.AddFile("C:\\TestProject\\TestFile.cs", testCode);
        
        // Act
        service.RemoveProperty("TestFile.cs", "TestClass", "Name");
        
        // Assert
        var result = fileSystem.GetFile("C:\\TestProject\\TestFile.cs").TextContents;
        result.Should().NotContain("public string Name { get; set; }");
        result.Should().Contain("public int Age { get; set; }");
    }     
[Fact]
    public void ReplaceProperty_ShouldReplaceExistingProperty()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var pathService = new PathService("C:\\TestProject");
        var service = new CSharpService(fileSystem, pathService);
        
        var testCode = @"using System;

namespace TestApp
{
    public class TestClass
    {
        public string Name { get; set; }
        
        public void SomeMethod()
        {
            Console.WriteLine(""Hello World"");
        }
    }
}";
        fileSystem.AddFile("C:\\TestProject\\TestFile.cs", testCode);
        
        // Act
        service.ReplaceProperty("TestFile.cs", "TestClass", "Name", "public string FullName { get; set; }");
        
        // Assert
        var result = fileSystem.GetFile("C:\\TestProject\\TestFile.cs").TextContents;
        result.Should().NotContain("public string Name { get; set; }");
        result.Should().Contain("public string FullName { get; set; }");
    } }
