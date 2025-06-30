using CodeEditor.MCP.Models;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Tools;
using FluentAssertions;
using Moq;

namespace CodeEditor.MCP.Tests;

public class AnalyzeFileTest
{
    private readonly Mock<ICodeStructureService> _mockCodeStructureService;

    public AnalyzeFileTest()
    {
        _mockCodeStructureService = new Mock<ICodeStructureService>();
    }

    [Fact]
    public void AnalyzeFile_AnonymizesMethodNames()
    {
        // Arrange
        var path = "test.cs";
        var methodWithName = new CodeMethodDefinition
        {
            Name = "TestMethod",
            Visibility = "public",
            ReturnType = "void",
            IsStatic = false
        };

        var typeWithMethod = new CodeTypeDefinition
        {
            Name = "TestClass",
            Kind = CodeTypeKind.Class,
            FilePath = path
        };
        typeWithMethod.Members.Methods.Add(methodWithName);

        var expectedTypes = new List<CodeTypeDefinition> { typeWithMethod };

        _mockCodeStructureService.Setup(x => x.ParseAllTypes(path))
            .Returns(expectedTypes);

        // Act
        var result = CSharpTools.AnalyzeFile(_mockCodeStructureService.Object, path);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("TestClass");
        result[0].Members.Methods.Should().HaveCount(1);
        result[0].Members.Methods[0].Name.Should().BeEmpty(); // Method name should be anonymized
        result[0].Members.Methods[0].Visibility.Should().Be("public");
        result[0].Members.Methods[0].ReturnType.Should().Be("void");
        _mockCodeStructureService.Verify(x => x.ParseAllTypes(path), Times.Once);
    }

    [Fact]
    public void AnalyzeFile_PreservesOtherMemberNames()
    {
        // Arrange
        var path = "test.cs";
        var property = new CodePropertyDefinition
        {
            Name = "TestProperty",
            Type = "string",
            Visibility = "public"
        };

        var field = new CodeFieldDefinition
        {
            Name = "TestField",
            Type = "int",
            Visibility = "private"
        };

        var typeWithMembers = new CodeTypeDefinition
        {
            Name = "TestClass",
            Kind = CodeTypeKind.Class,
            FilePath = path
        };
        typeWithMembers.Members.Properties.Add(property);
        typeWithMembers.Members.Fields.Add(field);

        var expectedTypes = new List<CodeTypeDefinition> { typeWithMembers };

        _mockCodeStructureService.Setup(x => x.ParseAllTypes(path))
            .Returns(expectedTypes);

        // Act
        var result = CSharpTools.AnalyzeFile(_mockCodeStructureService.Object, path);

        // Assert
        result.Should().HaveCount(1);
        result[0].Members.Properties.Should().HaveCount(1);
        result[0].Members.Properties[0].Name.Should().Be("TestProperty"); // Property names should be preserved
        result[0].Members.Fields.Should().HaveCount(1);
        result[0].Members.Fields[0].Name.Should().Be("TestField"); // Field names should be preserved
    }
}
