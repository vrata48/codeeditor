using System.IO.Abstractions.TestingHelpers;
using CodeEditor.MCP.Services;
using FluentAssertions;

namespace CodeEditor.MCP.Tests;

public class CSharpServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testProjectDirectory;
    private PathService _pathService = null!;
    private CSharpService _csharpService = null!;
    private MockFileSystem _fileSystem = null!;

    public CSharpServiceTests()
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
        _csharpService = new CSharpService(_fileSystem, _pathService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
[Fact]
    public void AnalyzeFile_SimpleClass_ReturnsClassAndMethods()
    {
        // Arrange
        var code = @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public void Method1()
        {
        }
        
        public int Method2(string param1, int param2)
        {
            return 0;
        }
        
        private string _field;
        
        public string Property { get; set; }
    }
}";
        var filePath = Path.Combine(_testProjectDirectory, "TestClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(code));

        // Act
        var result = _csharpService.AnalyzeFile("TestClass.cs");

        // Assert
        result.Should().Contain("\"name\": \"TestClass\"");
        result.Should().Contain("\"name\": \"Method1\"");
        result.Should().Contain("\"name\": \"Method2\"");
        result.Should().Contain("\"name\": \"Property\"");
    } 
[Fact]
    public void AnalyzeFile_MultipleClasses_ReturnsAllClasses()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class FirstClass
    {
        public void FirstMethod() { }
    }
    
    public class SecondClass
    {
        public string SecondMethod(int param) { return string.Empty; }
    }
}";
        var filePath = Path.Combine(_testProjectDirectory, "MultipleClasses.cs");
        _fileSystem.AddFile(filePath, new MockFileData(code));

        // Act
        var result = _csharpService.AnalyzeFile("MultipleClasses.cs");

        // Assert
        result.Should().Contain("\"name\": \"FirstClass\"");
        result.Should().Contain("\"name\": \"SecondClass\"");
        result.Should().Contain("\"name\": \"FirstMethod\"");
        result.Should().Contain("\"name\": \"SecondMethod\"");
    } 
[Fact]
    public void AnalyzeFile_ClassWithoutMethods_ReturnsOnlyClass()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class EmptyClass
    {
        public string Property { get; set; }
        private int _field;
    }
}";
        var filePath = Path.Combine(_testProjectDirectory, "EmptyClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(code));

        // Act
        var result = _csharpService.AnalyzeFile("EmptyClass.cs");

        // Assert
        result.Should().Contain("\"name\": \"EmptyClass\"");
        result.Should().Contain("\"methods\": []"); // Should contain empty methods array
    }     [Fact]
    public void AddMethod_ToExistingClass_AddsMethodSuccessfully()
    {
        // Arrange
        var originalCode = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void ExistingMethod()
        {
        }
    }
}";
        var methodCode = @"
        public string NewMethod(int param)
        {
            return param.ToString();
        }";
        
        var filePath = Path.Combine(_testProjectDirectory, "TestClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(originalCode));

        // Act
        _csharpService.AddMethod("TestClass.cs", "TestClass", methodCode);

        // Assert
        var updatedCode = _fileSystem.File.ReadAllText(filePath);
        updatedCode.Should().Contain("ExistingMethod");
        updatedCode.Should().Contain("NewMethod");
        updatedCode.Should().Contain("int param");
    }

    [Fact]
    public void AddMethod_ClassNotFound_DoesNotModifyFile()
    {
        // Arrange
        var originalCode = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void ExistingMethod() { }
    }
}";
        var methodCode = @"
        public string NewMethod() { return string.Empty; }";
        
        var filePath = Path.Combine(_testProjectDirectory, "TestClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(originalCode));

        // Act
        _csharpService.AddMethod("TestClass.cs", "NonExistentClass", methodCode);

        // Assert
        var updatedCode = _fileSystem.File.ReadAllText(filePath);
        updatedCode.Should().Be(originalCode); // Should remain unchanged
    }

    [Fact]
    public void ReplaceMethod_ExistingMethod_ReplacesSuccessfully()
    {
        // Arrange
        var originalCode = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void MethodToReplace()
        {
            Console.WriteLine(""Old implementation"");
        }
        
        public void OtherMethod()
        {
        }
    }
}";
        var newMethodCode = @"
        public void MethodToReplace()
        {
            Console.WriteLine(""New implementation"");
        }";
        
        var filePath = Path.Combine(_testProjectDirectory, "TestClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(originalCode));

        // Act
        _csharpService.ReplaceMethod("TestClass.cs", "TestClass", "MethodToReplace", newMethodCode);

        // Assert
        var updatedCode = _fileSystem.File.ReadAllText(filePath);
        updatedCode.Should().Contain("New implementation");
        updatedCode.Should().NotContain("Old implementation");
        updatedCode.Should().Contain("OtherMethod"); // Other methods should remain
    }

    [Fact]
    public void ReplaceMethod_MethodNotFound_DoesNotModifyFile()
    {
        // Arrange
        var originalCode = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void ExistingMethod() { }
    }
}";
        var newMethodCode = @"public void NewMethod() { }";
        
        var filePath = Path.Combine(_testProjectDirectory, "TestClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(originalCode));

        // Act
        _csharpService.ReplaceMethod("TestClass.cs", "TestClass", "NonExistentMethod", newMethodCode);

        // Assert
        var updatedCode = _fileSystem.File.ReadAllText(filePath);
        updatedCode.Should().Be(originalCode); // Should remain unchanged
    }

    [Fact]
    public void RemoveMethod_ExistingMethod_RemovesSuccessfully()
    {
        // Arrange
        var originalCode = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void MethodToRemove()
        {
            Console.WriteLine(""This method will be removed"");
        }
        
        public void MethodToKeep()
        {
            Console.WriteLine(""This method will stay"");
        }
    }
}";
        
        var filePath = Path.Combine(_testProjectDirectory, "TestClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(originalCode));

        // Act
        _csharpService.RemoveMethod("TestClass.cs", "TestClass", "MethodToRemove");

        // Assert
        var updatedCode = _fileSystem.File.ReadAllText(filePath);
        updatedCode.Should().NotContain("MethodToRemove");
        updatedCode.Should().NotContain("This method will be removed");
        updatedCode.Should().Contain("MethodToKeep");
        updatedCode.Should().Contain("This method will stay");
    }

    [Fact]
    public void RemoveMethod_MethodNotFound_DoesNotModifyFile()
    {
        // Arrange
        var originalCode = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void ExistingMethod() { }
    }
}";
        
        var filePath = Path.Combine(_testProjectDirectory, "TestClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(originalCode));

        // Act
        _csharpService.RemoveMethod("TestClass.cs", "TestClass", "NonExistentMethod");

        // Assert
        var updatedCode = _fileSystem.File.ReadAllText(filePath);
        updatedCode.Should().Be(originalCode); // Should remain unchanged
    }
[Fact]
    public void AnalyzeFile_ComplexMethods_ParsesParametersCorrectly()
    {
        // Arrange
        var code = @"
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class ComplexClass
    {
        public async Task<string> ComplexMethod(
            string stringParam,
            int intParam,
            List<string> listParam,
            Dictionary<string, object> dictParam)
        {
            return await Task.FromResult(""result"");
        }
        
        public void GenericMethod<T>(T item) where T : class
        {
        }
    }
}";
        var filePath = Path.Combine(_testProjectDirectory, "ComplexClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(code));

        // Act
        var result = _csharpService.AnalyzeFile("ComplexClass.cs");

        // Assert
        result.Should().Contain("\"name\": \"ComplexClass\"");
        result.Should().Contain("\"name\": \"ComplexMethod\"");
        result.Should().Contain("\"name\": \"stringParam\"");
        result.Should().Contain("\"name\": \"intParam\"");
        result.Should().Contain("\"name\": \"GenericMethod\"");
    } 
[Fact]
    public void AnalyzeFile_NestedClasses_ReturnsAllClasses()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class OuterClass
    {
        public void OuterMethod() { }
        
        public class NestedClass
        {
            public void NestedMethod() { }
        }
    }
}";
        var filePath = Path.Combine(_testProjectDirectory, "NestedClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(code));

        // Act
        var result = _csharpService.AnalyzeFile("NestedClass.cs");

        // Assert
        result.Should().Contain("\"name\": \"OuterClass\"");
        result.Should().Contain("\"name\": \"NestedClass\"");
        result.Should().Contain("\"name\": \"OuterMethod\"");
        result.Should().Contain("\"name\": \"NestedMethod\"");
    } 
    [Fact]
    public void AddMethod_WithComplexMethodCode_HandlesCorrectly()
    {
        // Arrange
        var originalCode = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void ExistingMethod() { }
    }
}";
        var complexMethodCode = @"
        /// <summary>
        /// A complex method with documentation
        /// </summary>
        /// <param name=""input"">The input parameter</param>
        /// <returns>A task representing the async operation</returns>
        [Obsolete(""This is just a test"")]
        public async Task<string> ComplexNewMethod(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException(""Input cannot be null or empty"");
                
            return await Task.FromResult(input.ToUpper());
        }";
        
        var filePath = Path.Combine(_testProjectDirectory, "TestClass.cs");
        _fileSystem.AddFile(filePath, new MockFileData(originalCode));

        // Act
        _csharpService.AddMethod("TestClass.cs", "TestClass", complexMethodCode);

        // Assert
        var updatedCode = _fileSystem.File.ReadAllText(filePath);
        updatedCode.Should().Contain("ComplexNewMethod");
        updatedCode.Should().Contain("summary");
        updatedCode.Should().Contain("Obsolete");
        updatedCode.Should().Contain("ArgumentException");
    }
}
