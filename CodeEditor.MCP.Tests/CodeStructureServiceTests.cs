using CodeEditor.MCP.Models;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Services.CodeStructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace CodeEditor.MCP.Tests;

public class CodeStructureServiceTests
{
    private readonly Mock<ICodeAnalysisService> _mockAnalysisService;
    private readonly Mock<ICodeModificationService> _mockModificationService;
    private readonly Mock<ICodeQueryService> _mockQueryService;
    private readonly Mock<ICodeValidationService> _mockValidationService;
    private readonly Mock<ICodeRefactoringService> _mockRefactoringService;
    private readonly Mock<IBatchOperationsService> _mockBatchOperationsService;
    private readonly Mock<ICodeGenerationService> _mockGenerationService;
    private readonly CodeStructureService _service;

    public CodeStructureServiceTests()
    {
        _mockAnalysisService = new Mock<ICodeAnalysisService>();
        _mockModificationService = new Mock<ICodeModificationService>();
        _mockQueryService = new Mock<ICodeQueryService>();
        _mockValidationService = new Mock<ICodeValidationService>();
        _mockRefactoringService = new Mock<ICodeRefactoringService>();
        _mockBatchOperationsService = new Mock<IBatchOperationsService>();
        _mockGenerationService = new Mock<ICodeGenerationService>();

        _service = new CodeStructureService(
            _mockAnalysisService.Object,
            _mockModificationService.Object,
            _mockQueryService.Object,
            _mockValidationService.Object,
            _mockRefactoringService.Object,
            _mockBatchOperationsService.Object,
            _mockGenerationService.Object);
    }

    #region Analysis Tests

    [Fact]
    public void ParseType_ShouldCallAnalysisService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var expectedResult = new CodeTypeDefinition { Name = typeName };
        _mockAnalysisService.Setup(x => x.ParseType(filePath, typeName)).Returns(expectedResult);

        // Act
        var result = _service.ParseType(filePath, typeName);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockAnalysisService.Verify(x => x.ParseType(filePath, typeName), Times.Once);
    }

    [Fact]
    public void ParseAllTypes_ShouldCallAnalysisService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var expectedResult = new List<CodeTypeDefinition>
        {
            new() { Name = "Class1" },
            new() { Name = "Class2" }
        };
        _mockAnalysisService.Setup(x => x.ParseAllTypes(filePath)).Returns(expectedResult);

        // Act
        var result = _service.ParseAllTypes(filePath);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockAnalysisService.Verify(x => x.ParseAllTypes(filePath), Times.Once);
    }

    [Fact]
    public void AnalyzeProject_ShouldCallAnalysisServiceWithDefaultPath()
    {
        // Arrange
        var expectedResult = new ProjectStructure();
        _mockAnalysisService.Setup(x => x.AnalyzeProject(".")).Returns(expectedResult);

        // Act
        var result = _service.AnalyzeProject();

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockAnalysisService.Verify(x => x.AnalyzeProject("."), Times.Once);
    }

    [Fact]
    public void AnalyzeProject_ShouldCallAnalysisServiceWithSpecifiedPath()
    {
        // Arrange
        var projectPath = "/custom/path";
        var expectedResult = new ProjectStructure();
        _mockAnalysisService.Setup(x => x.AnalyzeProject(projectPath)).Returns(expectedResult);

        // Act
        var result = _service.AnalyzeProject(projectPath);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockAnalysisService.Verify(x => x.AnalyzeProject(projectPath), Times.Once);
    }

    #endregion

    #region Query Tests

    [Fact]
    public void FindTypesByName_ShouldCallQueryService()
    {
        // Arrange
        var pattern = "Test*";
        var expectedResult = new List<CodeTypeDefinition> { new() { Name = "TestClass" } };
        _mockQueryService.Setup(x => x.FindTypesByName(pattern)).Returns(expectedResult);

        // Act
        var result = _service.FindTypesByName(pattern);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockQueryService.Verify(x => x.FindTypesByName(pattern), Times.Once);
    }

    [Fact]
    public void FindMethodsBySignature_ShouldCallQueryService()
    {
        // Arrange
        var returnType = "string";
        var name = "GetValue";
        var parameterTypes = new[] { "int", "bool" };
        var expectedResult = new List<CodeMethodDefinition> 
        { 
            new() { Name = name, ReturnType = returnType } 
        };
        _mockQueryService.Setup(x => x.FindMethodsBySignature(returnType, name, parameterTypes))
                        .Returns(expectedResult);

        // Act
        var result = _service.FindMethodsBySignature(returnType, name, parameterTypes);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockQueryService.Verify(x => x.FindMethodsBySignature(returnType, name, parameterTypes), Times.Once);
    }

    [Fact]
    public void FindTypesWithAttribute_ShouldCallQueryService()
    {
        // Arrange
        var attributeName = "Serializable";
        var expectedResult = new List<CodeTypeDefinition> { new() { Name = "TestClass" } };
        _mockQueryService.Setup(x => x.FindTypesWithAttribute(attributeName)).Returns(expectedResult);

        // Act
        var result = _service.FindTypesWithAttribute(attributeName);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockQueryService.Verify(x => x.FindTypesWithAttribute(attributeName), Times.Once);
    }

    [Fact]
    public void FindAllReferences_ShouldCallQueryService()
    {
        // Arrange
        var typeName = "TestClass";
        var memberName = "TestMethod";
        var expectedResult = new List<string> { "File1.cs:10", "File2.cs:25" };
        _mockQueryService.Setup(x => x.FindAllReferences(typeName, memberName)).Returns(expectedResult);

        // Act
        var result = _service.FindAllReferences(typeName, memberName);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockQueryService.Verify(x => x.FindAllReferences(typeName, memberName), Times.Once);
    }

    [Fact]
    public void GetChangeImpact_ShouldCallQueryService()
    {
        // Arrange
        var typeName = "TestClass";
        var memberName = "TestMethod";
        var expectedResult = new List<string> { "File1.cs", "File2.cs" };
        _mockQueryService.Setup(x => x.GetChangeImpact(typeName, memberName)).Returns(expectedResult);

        // Act
        var result = _service.GetChangeImpact(typeName, memberName);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockQueryService.Verify(x => x.GetChangeImpact(typeName, memberName), Times.Once);
    }

    [Fact]
    public void GetDependentFiles_ShouldCallQueryService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var expectedResult = new List<string> { "File1.cs", "File2.cs" };
        _mockQueryService.Setup(x => x.GetDependentFiles(filePath)).Returns(expectedResult);

        // Act
        var result = _service.GetDependentFiles(filePath);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockQueryService.Verify(x => x.GetDependentFiles(filePath), Times.Once);
    }

    [Fact]
    public void GetUsages_WithMemberName_ShouldCallQueryService()
    {
        // Arrange
        var typeName = "TestClass";
        var memberName = "TestMethod";
        var expectedResult = new List<string> { "Usage1", "Usage2" };
        _mockQueryService.Setup(x => x.GetUsages(typeName, memberName)).Returns(expectedResult);

        // Act
        var result = _service.GetUsages(typeName, memberName);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockQueryService.Verify(x => x.GetUsages(typeName, memberName), Times.Once);
    }

    [Fact]
    public void GetUsages_WithoutMemberName_ShouldCallQueryService()
    {
        // Arrange
        var typeName = "TestClass";
        var expectedResult = new List<string> { "Usage1", "Usage2" };
        _mockQueryService.Setup(x => x.GetUsages(typeName, null)).Returns(expectedResult);

        // Act
        var result = _service.GetUsages(typeName);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockQueryService.Verify(x => x.GetUsages(typeName, null), Times.Once);
    }

    #endregion

    #region Modification Tests

    [Fact]
    public void ModifyType_ShouldCallModificationService()
    {
        // Arrange
        var type = new CodeTypeDefinition { Name = "TestClass" };

        // Act
        _service.ModifyType(type);

        // Assert
        _mockModificationService.Verify(x => x.ModifyType(type), Times.Once);
    }

    [Fact]
    public void RenameSymbol_ShouldCallRefactoringService()
    {
        // Arrange
        var oldName = "OldName";
        var newName = "NewName";
        var typeName = "TestClass";

        // Act
        _service.RenameSymbol(oldName, newName, typeName);

        // Assert
        _mockRefactoringService.Verify(x => x.RenameSymbol(oldName, newName, typeName), Times.Once);
    }

    #endregion

    #region Method Management Tests

    [Fact]
    public void AddMethod_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var method = new CodeMethodDefinition { Name = "TestMethod" };

        // Act
        _service.AddMethod(filePath, typeName, method);

        // Assert
        _mockModificationService.Verify(x => x.AddMethod(filePath, typeName, method), Times.Once);
    }

    [Fact]
    public void RemoveMethod_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "TestMethod";

        // Act
        _service.RemoveMethod(filePath, typeName, methodName);

        // Assert
        _mockModificationService.Verify(x => x.RemoveMethod(filePath, typeName, methodName), Times.Once);
    }

    [Fact]
    public void ReplaceMethod_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var oldMethodName = "OldMethod";
        var newMethod = new CodeMethodDefinition { Name = "NewMethod" };

        // Act
        _service.ReplaceMethod(filePath, typeName, oldMethodName, newMethod);

        // Assert
        _mockModificationService.Verify(x => x.ReplaceMethod(filePath, typeName, oldMethodName, newMethod), Times.Once);
    }

    [Fact]
    public void GetMethod_WhenMethodExists_ShouldReturnMethod()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "TestMethod";
        var expectedMethod = new CodeMethodDefinition { Name = methodName };
        var type = new CodeTypeDefinition 
        { 
            Name = typeName,
            Members = new CodeMemberCollection
            {
                Methods = new List<CodeMethodDefinition> { expectedMethod }
            }
        };
        _mockAnalysisService.Setup(x => x.ParseType(filePath, typeName)).Returns(type);

        // Act
        var result = _service.GetMethod(filePath, typeName, methodName);

        // Assert
        result.Should().BeSameAs(expectedMethod);
    }

    [Fact]
    public void GetMethod_WhenMethodDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "NonExistentMethod";
        var type = new CodeTypeDefinition 
        { 
            Name = typeName,
            Members = new CodeMemberCollection
            {
                Methods = new List<CodeMethodDefinition>()
            }
        };
        _mockAnalysisService.Setup(x => x.ParseType(filePath, typeName)).Returns(type);

        // Act
        var result = _service.GetMethod(filePath, typeName, methodName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetMethodBody_WhenMethodExists_ShouldReturnBody()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "TestMethod";
        var expectedBody = "return true;";
        var method = new CodeMethodDefinition { Name = methodName, Body = expectedBody };
        var type = new CodeTypeDefinition 
        { 
            Name = typeName,
            Members = new CodeMemberCollection
            {
                Methods = new List<CodeMethodDefinition> { method }
            }
        };
        _mockAnalysisService.Setup(x => x.ParseType(filePath, typeName)).Returns(type);

        // Act
        var result = _service.GetMethodBody(filePath, typeName, methodName);

        // Assert
        result.Should().Be(expectedBody);
    }

    [Fact]
    public void GetMethodBody_WhenMethodDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "NonExistentMethod";
        var type = new CodeTypeDefinition 
        { 
            Name = typeName,
            Members = new CodeMemberCollection
            {
                Methods = new List<CodeMethodDefinition>()
            }
        };
        _mockAnalysisService.Setup(x => x.ParseType(filePath, typeName)).Returns(type);

        // Act & Assert
        _service.Invoking(s => s.GetMethodBody(filePath, typeName, methodName))
               .Should().Throw<InvalidOperationException>()
               .WithMessage($"Method '{methodName}' not found");
    }

    [Fact]
    public void UpdateMethodBody_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "TestMethod";
        var newBody = "return false;";

        // Act
        _service.UpdateMethodBody(filePath, typeName, methodName, newBody);

        // Assert
        _mockModificationService.Verify(x => x.UpdateMethodBody(filePath, typeName, methodName, newBody), Times.Once);
    }

    #endregion

    #region Property Management Tests

    [Fact]
    public void AddProperty_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var property = new CodePropertyDefinition { Name = "TestProperty" };

        // Act
        _service.AddProperty(filePath, typeName, property);

        // Assert
        _mockModificationService.Verify(x => x.AddProperty(filePath, typeName, property), Times.Once);
    }

    [Fact]
    public void RemoveProperty_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var propertyName = "TestProperty";

        // Act
        _service.RemoveProperty(filePath, typeName, propertyName);

        // Assert
        _mockModificationService.Verify(x => x.RemoveProperty(filePath, typeName, propertyName), Times.Once);
    }

    [Fact]
    public void ReplaceProperty_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var oldPropertyName = "OldProperty";
        var newProperty = new CodePropertyDefinition { Name = "NewProperty" };

        // Act
        _service.ReplaceProperty(filePath, typeName, oldPropertyName, newProperty);

        // Assert
        _mockModificationService.Verify(x => x.ReplaceProperty(filePath, typeName, oldPropertyName, newProperty), Times.Once);
    }

    [Fact]
    public void GetProperty_WhenPropertyExists_ShouldReturnProperty()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var propertyName = "TestProperty";
        var expectedProperty = new CodePropertyDefinition { Name = propertyName };
        var type = new CodeTypeDefinition 
        { 
            Name = typeName,
            Members = new CodeMemberCollection
            {
                Properties = new List<CodePropertyDefinition> { expectedProperty }
            }
        };
        _mockAnalysisService.Setup(x => x.ParseType(filePath, typeName)).Returns(type);

        // Act
        var result = _service.GetProperty(filePath, typeName, propertyName);

        // Assert
        result.Should().BeSameAs(expectedProperty);
    }

    [Fact]
    public void GetProperty_WhenPropertyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var propertyName = "NonExistentProperty";
        var type = new CodeTypeDefinition 
        { 
            Name = typeName,
            Members = new CodeMemberCollection
            {
                Properties = new List<CodePropertyDefinition>()
            }
        };
        _mockAnalysisService.Setup(x => x.ParseType(filePath, typeName)).Returns(type);

        // Act
        var result = _service.GetProperty(filePath, typeName, propertyName);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Field Management Tests

    [Fact]
    public void AddField_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var field = new CodeFieldDefinition { Name = "TestField" };

        // Act
        _service.AddField(filePath, typeName, field);

        // Assert
        _mockModificationService.Verify(x => x.AddField(filePath, typeName, field), Times.Once);
    }

    [Fact]
    public void RemoveField_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var fieldName = "TestField";

        // Act
        _service.RemoveField(filePath, typeName, fieldName);

        // Assert
        _mockModificationService.Verify(x => x.RemoveField(filePath, typeName, fieldName), Times.Once);
    }

    [Fact]
    public void ReplaceField_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var oldFieldName = "OldField";
        var newField = new CodeFieldDefinition { Name = "NewField" };

        // Act
        _service.ReplaceField(filePath, typeName, oldFieldName, newField);

        // Assert
        _mockModificationService.Verify(x => x.ReplaceField(filePath, typeName, oldFieldName, newField), Times.Once);
    }

    #endregion

    #region Interface Management Tests

    [Fact]
    public void CreateInterface_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var interfaceName = "ITestInterface";
        var interfaceDefinition = new CodeTypeDefinition { Name = interfaceName, Kind = CodeTypeKind.Interface };

        // Act
        _service.CreateInterface(filePath, interfaceName, interfaceDefinition);

        // Assert
        _mockModificationService.Verify(x => x.CreateType(filePath, interfaceDefinition), Times.Once);
    }

    [Fact]
    public void AddMethodToInterface_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var interfaceName = "ITestInterface";
        var method = new CodeMethodDefinition { Name = "TestMethod" };

        // Act
        _service.AddMethodToInterface(filePath, interfaceName, method);

        // Assert
        _mockModificationService.Verify(x => x.AddMethod(filePath, interfaceName, method), Times.Once);
    }

    [Fact]
    public void RemoveMethodFromInterface_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var interfaceName = "ITestInterface";
        var methodName = "TestMethod";

        // Act
        _service.RemoveMethodFromInterface(filePath, interfaceName, methodName);

        // Assert
        _mockModificationService.Verify(x => x.RemoveMethod(filePath, interfaceName, methodName), Times.Once);
    }

    [Fact]
    public void AddPropertyToInterface_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var interfaceName = "ITestInterface";
        var property = new CodePropertyDefinition { Name = "TestProperty" };

        // Act
        _service.AddPropertyToInterface(filePath, interfaceName, property);

        // Assert
        _mockModificationService.Verify(x => x.AddProperty(filePath, interfaceName, property), Times.Once);
    }

    #endregion

    #region Type Management Tests

    [Fact]
    public void CreateType_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var type = new CodeTypeDefinition { Name = "TestClass" };

        // Act
        _service.CreateType(filePath, type);

        // Assert
        _mockModificationService.Verify(x => x.CreateType(filePath, type), Times.Once);
    }

    [Fact]
    public void RemoveType_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";

        // Act
        _service.RemoveType(filePath, typeName);

        // Assert
        _mockModificationService.Verify(x => x.RemoveType(filePath, typeName), Times.Once);
    }

    [Fact]
    public void AddInterface_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var interfaceName = "ITestInterface";

        // Act
        _service.AddInterface(filePath, typeName, interfaceName);

        // Assert
        _mockModificationService.Verify(x => x.AddInterface(filePath, typeName, interfaceName), Times.Once);
    }

    [Fact]
    public void RemoveInterface_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var interfaceName = "ITestInterface";

        // Act
        _service.RemoveInterface(filePath, typeName, interfaceName);

        // Assert
        _mockModificationService.Verify(x => x.RemoveInterface(filePath, typeName, interfaceName), Times.Once);
    }

    [Fact]
    public void ChangeBaseClass_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var newBaseClass = "BaseClass";

        // Act
        _service.ChangeBaseClass(filePath, typeName, newBaseClass);

        // Assert
        _mockModificationService.Verify(x => x.ChangeBaseClass(filePath, typeName, newBaseClass), Times.Once);
    }

    #endregion

    #region Convenience Methods Tests

    [Fact]
    public void AddPublicMethod_ShouldCreateMethodAndCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "TestMethod";
        var returnType = "string";
        var body = "return \"test\";";
        var parameters = new[] { ("int", "id"), ("bool", "flag") };

        // Act
        _service.AddPublicMethod(filePath, typeName, methodName, returnType, body, parameters);

        // Assert
        _mockModificationService.Verify(x => x.AddMethod(filePath, typeName, It.Is<CodeMethodDefinition>(m =>
            m.Name == methodName &&
            m.ReturnType == returnType &&
            m.Visibility == "public" &&
            m.Body == body &&
            m.Parameters.Count == 2 &&
            m.Parameters[0].Type == "int" &&
            m.Parameters[0].Name == "id" &&
            m.Parameters[1].Type == "bool" &&
            m.Parameters[1].Name == "flag")), Times.Once);
    }

    [Fact]
    public void AddPrivateMethod_ShouldCreateMethodAndCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "TestMethod";
        var returnType = "void";
        var body = "// implementation";
        var parameters = new[] { ("string", "value") };

        // Act
        _service.AddPrivateMethod(filePath, typeName, methodName, returnType, body, parameters);

        // Assert
        _mockModificationService.Verify(x => x.AddMethod(filePath, typeName, It.Is<CodeMethodDefinition>(m =>
            m.Name == methodName &&
            m.ReturnType == returnType &&
            m.Visibility == "private" &&
            m.Body == body &&
            m.Parameters.Count == 1 &&
            m.Parameters[0].Type == "string" &&
            m.Parameters[0].Name == "value")), Times.Once);
    }

    [Fact]
    public void AddPublicProperty_ShouldCreatePropertyAndCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var propertyName = "TestProperty";
        var type = "string";

        // Act
        _service.AddPublicProperty(filePath, typeName, propertyName, type);

        // Assert
        _mockModificationService.Verify(x => x.AddProperty(filePath, typeName, It.Is<CodePropertyDefinition>(p =>
            p.Name == propertyName &&
            p.Type == type &&
            p.Visibility == "public" &&
            p.HasGetter == true &&
            p.HasSetter == true)), Times.Once);
    }

    [Fact]
    public void AddPublicProperty_WithCustomGetterSetter_ShouldCreatePropertyAndCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var propertyName = "ReadOnlyProperty";
        var type = "int";

        // Act
        _service.AddPublicProperty(filePath, typeName, propertyName, type, hasGetter: true, hasSetter: false);

        // Assert
        _mockModificationService.Verify(x => x.AddProperty(filePath, typeName, It.Is<CodePropertyDefinition>(p =>
            p.Name == propertyName &&
            p.Type == type &&
            p.Visibility == "public" &&
            p.HasGetter == true &&
            p.HasSetter == false)), Times.Once);
    }

    [Fact]
    public void AddPrivateField_ShouldCreateFieldAndCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var fieldName = "_testField";
        var type = "string";

        // Act
        _service.AddPrivateField(filePath, typeName, fieldName, type);

        // Assert
        _mockModificationService.Verify(x => x.AddField(filePath, typeName, It.Is<CodeFieldDefinition>(f =>
            f.Name == fieldName &&
            f.Type == type &&
            f.Visibility == "private" &&
            f.DefaultValue == null)), Times.Once);
    }

    [Fact]
    public void AddPrivateField_WithDefaultValue_ShouldCreateFieldAndCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var fieldName = "_counter";
        var type = "int";
        var defaultValue = "0";

        // Act
        _service.AddPrivateField(filePath, typeName, fieldName, type, defaultValue);

        // Assert
        _mockModificationService.Verify(x => x.AddField(filePath, typeName, It.Is<CodeFieldDefinition>(f =>
            f.Name == fieldName &&
            f.Type == type &&
            f.Visibility == "private" &&
            f.DefaultValue == defaultValue)), Times.Once);
    }

    #endregion

    #region Batch Operations Tests

    [Fact]
    public void AddMethods_ShouldCallBatchOperationsService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methods = new List<CodeMethodDefinition>
        {
            new() { Name = "Method1" },
            new() { Name = "Method2" }
        };

        // Act
        _service.AddMethods(filePath, typeName, methods);

        // Assert
        _mockBatchOperationsService.Verify(x => x.AddMethods(filePath, typeName, methods), Times.Once);
    }

    [Fact]
    public void AddProperties_ShouldCallBatchOperationsService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var properties = new List<CodePropertyDefinition>
        {
            new() { Name = "Property1" },
            new() { Name = "Property2" }
        };

        // Act
        _service.AddProperties(filePath, typeName, properties);

        // Assert
        _mockBatchOperationsService.Verify(x => x.AddProperties(filePath, typeName, properties), Times.Once);
    }

    [Fact]
    public void RemoveMethods_ShouldCallBatchOperationsService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodNames = new List<string> { "Method1", "Method2" };

        // Act
        _service.RemoveMethods(filePath, typeName, methodNames);

        // Assert
        _mockBatchOperationsService.Verify(x => x.RemoveMethods(filePath, typeName, methodNames), Times.Once);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void TypeExists_ShouldCallValidationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        _mockValidationService.Setup(x => x.TypeExists(filePath, typeName)).Returns(true);

        // Act
        var result = _service.TypeExists(filePath, typeName);

        // Assert
        result.Should().BeTrue();
        _mockValidationService.Verify(x => x.TypeExists(filePath, typeName), Times.Once);
    }

    [Fact]
    public void MethodExists_ShouldCallValidationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "TestMethod";
        _mockValidationService.Setup(x => x.MethodExists(filePath, typeName, methodName)).Returns(true);

        // Act
        var result = _service.MethodExists(filePath, typeName, methodName);

        // Assert
        result.Should().BeTrue();
        _mockValidationService.Verify(x => x.MethodExists(filePath, typeName, methodName), Times.Once);
    }

    [Fact]
    public void PropertyExists_ShouldCallValidationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var propertyName = "TestProperty";
        _mockValidationService.Setup(x => x.PropertyExists(filePath, typeName, propertyName)).Returns(false);

        // Act
        var result = _service.PropertyExists(filePath, typeName, propertyName);

        // Assert
        result.Should().BeFalse();
        _mockValidationService.Verify(x => x.PropertyExists(filePath, typeName, propertyName), Times.Once);
    }

    [Fact]
    public void ValidateModification_ShouldCallValidationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var operation = "AddMethod";
        var expectedResult = new List<string> { "Warning: Method already exists" };
        _mockValidationService.Setup(x => x.ValidateModification(filePath, typeName, operation)).Returns(expectedResult);

        // Act
        var result = _service.ValidateModification(filePath, typeName, operation);

        // Assert
        result.Should().BeSameAs(expectedResult);
        _mockValidationService.Verify(x => x.ValidateModification(filePath, typeName, operation), Times.Once);
    }

    #endregion

    #region Code Generation Tests

    [Fact]
    public void GenerateCode_ShouldCallGenerationService()
    {
        // Arrange
        var type = new CodeTypeDefinition { Name = "TestClass" };
        var expectedResult = "public class TestClass { }";
        _mockGenerationService.Setup(x => x.GenerateCode(type)).Returns(expectedResult);

        // Act
        var result = _service.GenerateCode(type);

        // Assert
        result.Should().Be(expectedResult);
        _mockGenerationService.Verify(x => x.GenerateCode(type), Times.Once);
    }

    [Fact]
    public void RegenerateFile_ShouldCallModificationService()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var types = new List<CodeTypeDefinition>
        {
            new() { Name = "Class1" },
            new() { Name = "Class2" }
        };

        // Act
        _service.RegenerateFile(filePath, types);

        // Assert
        _mockModificationService.Verify(x => x.RegenerateFile(filePath, types), Times.Once);
    }

    #endregion
}
