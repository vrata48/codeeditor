using CodeEditor.MCP.Models;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Tools;
using FluentAssertions;
using Moq;
using System.Text.Json;

namespace CodeEditor.MCP.Tests;

public class CSharpToolsTests
{
    private readonly Mock<ICodeStructureService> _mockCodeStructureService;
    private readonly Mock<ICSharpFormattingService> _mockFormattingService;

    public CSharpToolsTests()
    {
        _mockCodeStructureService = new Mock<ICodeStructureService>();
        _mockFormattingService = new Mock<ICSharpFormattingService>();
    }

    #region AnalyzeFile Tests

    [Fact]
    public void AnalyzeFile_ReturnsTypesFromService()
    {
        // Arrange
        var path = "test.cs";
        var expectedTypes = new List<CodeTypeDefinition>
        {
            new() { Name = "TestClass", Kind = CodeTypeKind.Class, FilePath = path },
            new() { Name = "ITestInterface", Kind = CodeTypeKind.Interface, FilePath = path }
        };

        _mockCodeStructureService.Setup(x => x.ParseAllTypes(path))
            .Returns(expectedTypes);

        // Act
        var result = CSharpTools.AnalyzeFile(_mockCodeStructureService.Object, path);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("TestClass");
        result[0].Kind.Should().Be(CodeTypeKind.Class);
        result[1].Name.Should().Be("ITestInterface");
        result[1].Kind.Should().Be(CodeTypeKind.Interface);
        _mockCodeStructureService.Verify(x => x.ParseAllTypes(path), Times.Once);
    }

    [Fact]
    public void AnalyzeFile_EmptyFile_ReturnsEmptyArray()
    {
        // Arrange
        var path = "empty.cs";
        _mockCodeStructureService.Setup(x => x.ParseAllTypes(path))
            .Returns(new List<CodeTypeDefinition>());

        // Act
        var result = CSharpTools.AnalyzeFile(_mockCodeStructureService.Object, path);

        // Assert
        result.Should().BeEmpty();
        _mockCodeStructureService.Verify(x => x.ParseAllTypes(path), Times.Once);
    }

    #endregion

    #region ReadMember Tests

    [Fact]
    public void ReadMember_Method_ReturnsMethodBody()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "TestMethod";
        var expectedBody = "return true;";

        _mockCodeStructureService.Setup(x => x.GetMethodBody(path, typeName, memberName))
            .Returns(expectedBody);

        // Act
        var result = CSharpTools.ReadMember(_mockCodeStructureService.Object, path, typeName, "method", memberName);

        // Assert
        result.Should().Be(expectedBody);
        _mockCodeStructureService.Verify(x => x.GetMethodBody(path, typeName, memberName), Times.Once);
    }

    [Fact]
    public void ReadMember_Property_ReturnsPropertyDefinition()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "TestProperty";
        var expectedProperty = new CodePropertyDefinition 
        { 
            Name = memberName, 
            Type = "string", 
            Visibility = "public",
            HasGetter = true,
            HasSetter = true
        };

        _mockCodeStructureService.Setup(x => x.GetProperty(path, typeName, memberName))
            .Returns(expectedProperty);

        // Act
        var result = CSharpTools.ReadMember(_mockCodeStructureService.Object, path, typeName, "property", memberName);

        // Assert
        result.Should().Contain(memberName);
        result.Should().Contain("string");
        _mockCodeStructureService.Verify(x => x.GetProperty(path, typeName, memberName), Times.Once);
    }

    [Fact]
    public void ReadMember_Field_ReturnsFieldDefinition()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "_testField";
        var expectedType = new CodeTypeDefinition
        {
            Name = typeName,
            Members = new CodeMemberCollection
            {
                Fields = new List<CodeFieldDefinition>
                {
                    new() { Name = memberName, Type = "string", Visibility = "private" }
                }
            }
        };

        _mockCodeStructureService.Setup(x => x.ParseType(path, typeName))
            .Returns(expectedType);

        // Act
        var result = CSharpTools.ReadMember(_mockCodeStructureService.Object, path, typeName, "field", memberName);

        // Assert
        result.Should().Contain(memberName);
        result.Should().Contain("string");
        _mockCodeStructureService.Verify(x => x.ParseType(path, typeName), Times.Once);
    }

    [Fact]
    public void ReadMember_PropertyNotFound_ReturnsNotFoundMessage()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "NonExistentProperty";

        _mockCodeStructureService.Setup(x => x.GetProperty(path, typeName, memberName))
            .Returns((CodePropertyDefinition?)null);

        // Act
        var result = CSharpTools.ReadMember(_mockCodeStructureService.Object, path, typeName, "property", memberName);

        // Assert
        result.Should().Be("Property not found");
        _mockCodeStructureService.Verify(x => x.GetProperty(path, typeName, memberName), Times.Once);
    }

    [Fact]
    public void ReadMember_FieldNotFound_ReturnsNotFoundMessage()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "nonExistentField";
        var expectedType = new CodeTypeDefinition
        {
            Name = typeName,
            Members = new CodeMemberCollection { Fields = new List<CodeFieldDefinition>() }
        };

        _mockCodeStructureService.Setup(x => x.ParseType(path, typeName))
            .Returns(expectedType);

        // Act
        var result = CSharpTools.ReadMember(_mockCodeStructureService.Object, path, typeName, "field", memberName);

        // Assert
        result.Should().Be("Field not found");
        _mockCodeStructureService.Verify(x => x.ParseType(path, typeName), Times.Once);
    }

    [Fact]
    public void ReadMember_InvalidMemberType_ThrowsArgumentException()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "testMember";

        // Act & Assert
        var action = () => CSharpTools.ReadMember(_mockCodeStructureService.Object, path, typeName, "invalid", memberName);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Unknown member type: invalid. Valid types: method, property, field");
    }

    #endregion

    #region AddMember Tests

    [Fact]
    public void AddMember_Method_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "TestMethod";
        var memberCode = "public void TestMethod()\n{\n    Console.WriteLine(\"Hello\");\n}";

        // Act
        CSharpTools.AddMember(_mockCodeStructureService.Object, path, typeName, "method", memberName, memberCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.AddMethod(path, typeName, It.Is<CodeMethodDefinition>(m => 
            m.Name == memberName && 
            m.ReturnType == "void" && 
            m.Visibility == "public")), Times.Once);
    }

    [Fact]
    public void AddMember_Property_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "TestProperty";
        var memberCode = "public string TestProperty { get; set; }";

        // Act
        CSharpTools.AddMember(_mockCodeStructureService.Object, path, typeName, "property", memberName, memberCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.AddProperty(path, typeName, It.Is<CodePropertyDefinition>(p => 
            p.Name == memberName && 
            p.Type == "string" && 
            p.Visibility == "public" &&
            p.HasGetter == true &&
            p.HasSetter == true)), Times.Once);
    }

    [Fact]
    public void AddMember_Field_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "_testField";
        var memberCode = "private readonly string _testField = \"default\";";

        // Act
        CSharpTools.AddMember(_mockCodeStructureService.Object, path, typeName, "field", memberName, memberCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.AddField(path, typeName, It.Is<CodeFieldDefinition>(f => 
            f.Name == memberName && 
            f.Type == "string" && 
            f.Visibility == "private" &&
            f.IsReadonly == true &&
            f.DefaultValue == "\"default\"")), Times.Once);
    }

    [Fact]
    public void AddMember_StaticAsyncMethod_ParsesCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "TestMethodAsync";
        var memberCode = "public static async Task<string> TestMethodAsync()\n{\n    return await GetDataAsync();\n}";

        // Act
        CSharpTools.AddMember(_mockCodeStructureService.Object, path, typeName, "method", memberName, memberCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.AddMethod(path, typeName, It.Is<CodeMethodDefinition>(m => 
            m.Name == memberName && 
            m.ReturnType == "Task<string>" && 
            m.Visibility == "public" &&
            m.IsStatic == true &&
            m.IsAsync == true)), Times.Once);
    }

    [Fact]
    public void AddMember_InvalidMemberType_ThrowsArgumentException()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "testMember";
        var memberCode = "some code";

        // Act & Assert
        var action = () => CSharpTools.AddMember(_mockCodeStructureService.Object, path, typeName, "invalid", memberName, memberCode);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Unknown member type: invalid. Valid types: method, property, field");
    }

    #endregion

    #region RemoveMember Tests

    [Fact]
    public void RemoveMember_Method_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "TestMethod";

        // Act
        CSharpTools.RemoveMember(_mockCodeStructureService.Object, path, typeName, "method", memberName);

        // Assert
        _mockCodeStructureService.Verify(x => x.RemoveMethod(path, typeName, memberName), Times.Once);
    }

    [Fact]
    public void RemoveMember_Property_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "TestProperty";

        // Act
        CSharpTools.RemoveMember(_mockCodeStructureService.Object, path, typeName, "property", memberName);

        // Assert
        _mockCodeStructureService.Verify(x => x.RemoveProperty(path, typeName, memberName), Times.Once);
    }

    [Fact]
    public void RemoveMember_Field_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "_testField";

        // Act
        CSharpTools.RemoveMember(_mockCodeStructureService.Object, path, typeName, "field", memberName);

        // Assert
        _mockCodeStructureService.Verify(x => x.RemoveField(path, typeName, memberName), Times.Once);
    }

    [Fact]
    public void RemoveMember_InvalidMemberType_ThrowsArgumentException()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var memberName = "testMember";

        // Act & Assert
        var action = () => CSharpTools.RemoveMember(_mockCodeStructureService.Object, path, typeName, "invalid", memberName);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Unknown member type: invalid. Valid types: method, property, field");
    }

    #endregion

    #region ReplaceMember Tests

    [Fact]
    public void ReplaceMember_Method_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var existingName = "OldMethod";
        var newName = "NewMethod";
        var memberCode = "public int NewMethod()\n{\n    return 42;\n}";

        // Act
        CSharpTools.ReplaceMember(_mockCodeStructureService.Object, path, typeName, "method", existingName, newName, memberCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.ReplaceMethod(path, typeName, existingName, It.Is<CodeMethodDefinition>(m => 
            m.Name == newName && 
            m.ReturnType == "int" && 
            m.Visibility == "public")), Times.Once);
    }

    [Fact]
    public void ReplaceMember_Property_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var existingName = "OldProperty";
        var newName = "NewProperty";
        var memberCode = "protected int NewProperty { get; }";

        // Act
        CSharpTools.ReplaceMember(_mockCodeStructureService.Object, path, typeName, "property", existingName, newName, memberCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.ReplaceProperty(path, typeName, existingName, It.Is<CodePropertyDefinition>(p => 
            p.Name == newName && 
            p.Type == "int" && 
            p.Visibility == "protected" &&
            p.HasGetter == true &&
            p.HasSetter == false)), Times.Once);
    }

    [Fact]
    public void ReplaceMember_Field_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var existingName = "oldField";
        var newName = "newField";
        var memberCode = "public static int newField;";

        // Act
        CSharpTools.ReplaceMember(_mockCodeStructureService.Object, path, typeName, "field", existingName, newName, memberCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.ReplaceField(path, typeName, existingName, It.Is<CodeFieldDefinition>(f => 
            f.Name == newName && 
            f.Type == "int" && 
            f.Visibility == "public" &&
            f.IsStatic == true)), Times.Once);
    }

    [Fact]
    public void ReplaceMember_InvalidMemberType_ThrowsArgumentException()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var existingName = "existing";
        var newName = "new";
        var memberCode = "some code";

        // Act & Assert
        var action = () => CSharpTools.ReplaceMember(_mockCodeStructureService.Object, path, typeName, "invalid", existingName, newName, memberCode);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Unknown member type: invalid. Valid types: method, property, field");
    }

    #endregion

    #region CreateType Tests

    [Fact]
    public void CreateType_Class_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestClass";
        var typeKind = "class";
        var typeCode = "public class TestClass { }";

        // Act
        CSharpTools.CreateType(_mockCodeStructureService.Object, path, typeName, typeKind, typeCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.CreateType(path, It.Is<CodeTypeDefinition>(t => 
            t.Name == typeName && 
            t.Kind == CodeTypeKind.Class && 
            t.Visibility == "public" &&
            t.FilePath == path)), Times.Once);
    }

    [Fact]
    public void CreateType_Interface_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "ITestInterface";
        var typeKind = "interface";
        var typeCode = "public interface ITestInterface { }";

        // Act
        CSharpTools.CreateType(_mockCodeStructureService.Object, path, typeName, typeKind, typeCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.CreateInterface(path, typeName, It.Is<CodeTypeDefinition>(t => 
            t.Name == typeName && 
            t.Kind == CodeTypeKind.Interface && 
            t.Visibility == "public" &&
            t.FilePath == path)), Times.Once);
    }

    [Fact]
    public void CreateType_Struct_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestStruct";
        var typeKind = "struct";
        var typeCode = "public struct TestStruct { }";

        // Act
        CSharpTools.CreateType(_mockCodeStructureService.Object, path, typeName, typeKind, typeCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.CreateType(path, It.Is<CodeTypeDefinition>(t => 
            t.Name == typeName && 
            t.Kind == CodeTypeKind.Struct && 
            t.Visibility == "public" &&
            t.FilePath == path)), Times.Once);
    }

    [Fact]
    public void CreateType_Enum_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestEnum";
        var typeKind = "enum";
        var typeCode = "public enum TestEnum { Value1, Value2 }";

        // Act
        CSharpTools.CreateType(_mockCodeStructureService.Object, path, typeName, typeKind, typeCode);

        // Assert
        _mockCodeStructureService.Verify(x => x.CreateType(path, It.Is<CodeTypeDefinition>(t => 
            t.Name == typeName && 
            t.Kind == CodeTypeKind.Enum && 
            t.Visibility == "public" &&
            t.FilePath == path)), Times.Once);
    }

    [Fact]
    public void CreateType_InvalidTypeKind_ThrowsArgumentException()
    {
        // Arrange
        var path = "test.cs";
        var typeName = "TestType";
        var typeKind = "invalid";
        var typeCode = "some code";

        // Act & Assert
        var action = () => CSharpTools.CreateType(_mockCodeStructureService.Object, path, typeName, typeKind, typeCode);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Unknown type kind: invalid");
    }

    #endregion

    #region FormatDocument Tests

    [Fact]
    public void FormatDocument_CallsServiceCorrectly()
    {
        // Arrange
        var path = "test.cs";

        // Act
        CSharpTools.FormatDocument(_mockFormattingService.Object, path);

        // Assert
        _mockFormattingService.Verify(x => x.FormatDocument(path), Times.Once);
    }

    #endregion

    #region FormatDirectory Tests

    [Fact]
    public void FormatDirectory_WithoutRecursion_CallsServiceCorrectly()
    {
        // Arrange
        var path = "src/";

        // Act
        CSharpTools.FormatDirectory(_mockFormattingService.Object, path, false);

        // Assert
        _mockFormattingService.Verify(x => x.FormatDirectory(path, false), Times.Once);
    }

    [Fact]
    public void FormatDirectory_WithRecursion_CallsServiceCorrectly()
    {
        // Arrange
        var path = "src/";

        // Act
        CSharpTools.FormatDirectory(_mockFormattingService.Object, path, true);

        // Assert
        _mockFormattingService.Verify(x => x.FormatDirectory(path, true), Times.Once);
    }

    [Fact]
    public void FormatDirectory_DefaultRecursion_CallsServiceWithFalse()
    {
        // Arrange
        var path = "src/";

        // Act
        CSharpTools.FormatDirectory(_mockFormattingService.Object, path);

        // Assert
        _mockFormattingService.Verify(x => x.FormatDirectory(path, false), Times.Once);
    }

    #endregion
}
