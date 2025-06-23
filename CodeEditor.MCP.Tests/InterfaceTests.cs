using CodeEditor.MCP.Services;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace CodeEditor.MCP.Tests;
public class InterfaceTests
{
    [Fact]
    public void CreateInterface_ShouldCreateNewFile_WhenFileDoesNotExist()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var pathService = new PathService("/temp");
        var csharpService = new CSharpService(fileSystem, pathService);
        var interfaceCode = @"public interface ITestInterface
{
    void TestMethod();
}";
        // Act
        csharpService.CreateInterface("TestInterface.cs", "ITestInterface", interfaceCode);
        // Assert
        Assert.True(fileSystem.File.Exists("/temp/TestInterface.cs"));
        var content = fileSystem.File.ReadAllText("/temp/TestInterface.cs");
        Assert.Contains("interface ITestInterface", content);
        Assert.Contains("void TestMethod();", content);
    }

    [Fact]
    public void AddMethodToInterface_ShouldAddMethodToExistingInterface()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var pathService = new PathService("/temp");
        var csharpService = new CSharpService(fileSystem, pathService);
        var initialContent = @"namespace TestNamespace;

public interface ITestInterface
{
    void ExistingMethod();
}";
        fileSystem.AddFile("/temp/TestInterface.cs", new MockFileData(initialContent));
        // Act
        csharpService.AddMethodToInterface("TestInterface.cs", "ITestInterface", "string NewMethod(int parameter);");
        // Assert
        var content = fileSystem.File.ReadAllText("/temp/TestInterface.cs");
        Assert.Contains("void ExistingMethod();", content);
        Assert.Contains("string NewMethod(int parameter);", content);
    }

    [Fact]
    public void ReplaceMethodInInterface_ShouldReplaceExistingMethod()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var pathService = new PathService("/temp");
        var csharpService = new CSharpService(fileSystem, pathService);
        var initialContent = @"namespace TestNamespace;

public interface ITestInterface
{
    void OldMethod();
    void AnotherMethod();
}";
        fileSystem.AddFile("/temp/TestInterface.cs", new MockFileData(initialContent));
        // Act
        csharpService.ReplaceMethodInInterface("TestInterface.cs", "ITestInterface", "OldMethod", "string NewMethod(int parameter);");
        // Assert
        var content = fileSystem.File.ReadAllText("/temp/TestInterface.cs");
        Assert.DoesNotContain("void OldMethod();", content);
        Assert.Contains("string NewMethod(int parameter);", content);
        Assert.Contains("void AnotherMethod();", content);
    }

    [Fact]
    public void RemoveMethodFromInterface_ShouldRemoveSpecifiedMethod()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var pathService = new PathService("/temp");
        var csharpService = new CSharpService(fileSystem, pathService);
        var initialContent = @"namespace TestNamespace;

public interface ITestInterface
{
    void MethodToRemove();
    void MethodToKeep();
}";
        fileSystem.AddFile("/temp/TestInterface.cs", new MockFileData(initialContent));
        // Act
        csharpService.RemoveMethodFromInterface("TestInterface.cs", "ITestInterface", "MethodToRemove");
        // Assert
        var content = fileSystem.File.ReadAllText("/temp/TestInterface.cs");
        Assert.DoesNotContain("void MethodToRemove();", content);
        Assert.Contains("void MethodToKeep();", content);
    }

    [Fact]
    public void AnalyzeFile_ShouldIncludeInterfaceInformation()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var pathService = new PathService("/temp");
        var csharpService = new CSharpService(fileSystem, pathService);
        var content = @"namespace TestNamespace;

public class TestClass
{
    public string Property { get; set; }
    public void Method() { }
}

public interface ITestInterface
{
    string InterfaceProperty { get; }
    void InterfaceMethod(int parameter);
}";
        fileSystem.AddFile("/temp/Test.cs", new MockFileData(content));
        // Act
        var result = csharpService.AnalyzeFile("Test.cs");
        // Assert
        Assert.Contains("\"name\": \"TestClass\"", result);
        Assert.Contains("\"name\": \"ITestInterface\"", result);
        Assert.Contains("\"name\": \"InterfaceProperty\"", result);
        Assert.Contains("\"name\": \"InterfaceMethod\"", result);
    }
}