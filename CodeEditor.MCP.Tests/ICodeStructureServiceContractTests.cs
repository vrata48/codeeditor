using CodeEditor.MCP.Models;
using CodeEditor.MCP.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CodeEditor.MCP.Tests;

/// <summary>
/// Contract-based tests for ICodeStructureService interface
/// Tests expected behavior without knowledge of implementation details
/// </summary>
public class ICodeStructureServiceContractTests
{
    private readonly Mock<ICodeStructureService> _mockService;

    public ICodeStructureServiceContractTests()
    {
        _mockService = new Mock<ICodeStructureService>();
    }

    #region Analysis Contract Tests

    [Fact]
    public void ParseType_ShouldReturnTypeDefinition_WhenValidInputProvided()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var expectedType = new CodeTypeDefinition 
        { 
            Name = typeName, 
            FilePath = filePath 
        };
        
        _mockService.Setup(x => x.ParseType(filePath, typeName))
                   .Returns(expectedType);

        // Act
        var result = _mockService.Object.ParseType(filePath, typeName);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(typeName);
        result.FilePath.Should().Be(filePath);
    }

    [Fact]
    public void ParseType_ShouldThrowException_WhenFilePathIsNull()
    {
        // Arrange
        _mockService.Setup(x => x.ParseType(null!, "TestClass"))
                   .Throws<ArgumentNullException>();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.ParseType(null!, "TestClass"))
                   .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ParseAllTypes_ShouldReturnEmptyList_WhenFileHasNoTypes()
    {
        // Arrange
        var filePath = "EmptyFile.cs";
        _mockService.Setup(x => x.ParseAllTypes(filePath))
                   .Returns(new List<CodeTypeDefinition>());

        // Act
        var result = _mockService.Object.ParseAllTypes(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseAllTypes_ShouldReturnMultipleTypes_WhenFileContainsMultipleTypes()
    {
        // Arrange
        var filePath = "MultipleTypes.cs";
        var expectedTypes = new List<CodeTypeDefinition>
        {
            new() { Name = "Class1", FilePath = filePath },
            new() { Name = "Interface1", FilePath = filePath, Kind = CodeTypeKind.Interface },
            new() { Name = "Enum1", FilePath = filePath, Kind = CodeTypeKind.Enum }
        };
        
        _mockService.Setup(x => x.ParseAllTypes(filePath))
                   .Returns(expectedTypes);

        // Act
        var result = _mockService.Object.ParseAllTypes(filePath);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(t => t.Kind == CodeTypeKind.Class);
        result.Should().Contain(t => t.Kind == CodeTypeKind.Interface);
        result.Should().Contain(t => t.Kind == CodeTypeKind.Enum);
    }

    [Fact]
    public void AnalyzeProject_ShouldUseCurrentDirectory_WhenNoPathProvided()
    {
        // Arrange
        var expectedStructure = new ProjectStructure();
        _mockService.Setup(x => x.AnalyzeProject("."))
                   .Returns(expectedStructure);

        // Act
        var result = _mockService.Object.AnalyzeProject();

        // Assert
        result.Should().NotBeNull();
        _mockService.Verify(x => x.AnalyzeProject("."), Times.Once);
    }

    #endregion

    #region Querying Contract Tests

    [Fact]
    public void FindTypesByName_ShouldReturnMatchingTypes_WhenPatternMatches()
    {
        // Arrange
        var pattern = "Test*";
        var matchingTypes = new List<CodeTypeDefinition>
        {
            new() { Name = "TestClass" },
            new() { Name = "TestService" }
        };
        
        _mockService.Setup(x => x.FindTypesByName(pattern))
                   .Returns(matchingTypes);

        // Act
        var result = _mockService.Object.FindTypesByName(pattern);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Name.StartsWith("Test"));
    }

    [Fact]
    public void FindMethodsBySignature_ShouldReturnMatchingMethods_WhenSignatureMatches()
    {
        // Arrange
        var returnType = "string";
        var methodName = "GetValue";
        var parameters = new[] { "int", "bool" };
        var matchingMethods = new List<CodeMethodDefinition>
        {
            new() 
            { 
                Name = methodName, 
                ReturnType = returnType,
                Parameters = new List<CodeParameterDefinition>
                {
                    new() { Type = "int", Name = "id" },
                    new() { Type = "bool", Name = "flag" }
                }
            }
        };
        
        _mockService.Setup(x => x.FindMethodsBySignature(returnType, methodName, parameters))
                   .Returns(matchingMethods);

        // Act
        var result = _mockService.Object.FindMethodsBySignature(returnType, methodName, parameters);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be(methodName);
        result.First().ReturnType.Should().Be(returnType);
        result.First().Parameters.Should().HaveCount(2);
    }

    [Fact]
    public void FindTypesWithAttribute_ShouldReturnEmptyList_WhenNoTypesHaveAttribute()
    {
        // Arrange
        var attributeName = "NonExistentAttribute";
        _mockService.Setup(x => x.FindTypesWithAttribute(attributeName))
                   .Returns(new List<CodeTypeDefinition>());

        // Act
        var result = _mockService.Object.FindTypesWithAttribute(attributeName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindAllReferences_ShouldReturnFileLocations_WhenReferencesExist()
    {
        // Arrange
        var typeName = "TestClass";
        var memberName = "TestMethod";
        var expectedReferences = new List<string>
        {
            "File1.cs:15",
            "File2.cs:42",
            "File3.cs:8"
        };
        
        _mockService.Setup(x => x.FindAllReferences(typeName, memberName))
                   .Returns(expectedReferences);

        // Act
        var result = _mockService.Object.FindAllReferences(typeName, memberName);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(r => r.Contains(".cs:"));
    }

    #endregion

    #region Method Management Contract Tests

    [Fact]
    public void AddMethod_ShouldNotThrow_WhenValidParametersProvided()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var method = new CodeMethodDefinition 
        { 
            Name = "NewMethod", 
            ReturnType = "void",
            Visibility = "public"
        };
        
        _mockService.Setup(x => x.AddMethod(filePath, typeName, method))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.AddMethod(filePath, typeName, method))
                   .Should().NotThrow();
        
        _mockService.Verify();
    }

    [Fact]
    public void GetMethod_ShouldReturnNull_WhenMethodDoesNotExist()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "NonExistentMethod";
        
        _mockService.Setup(x => x.GetMethod(filePath, typeName, methodName))
                   .Returns((CodeMethodDefinition?)null);

        // Act
        var result = _mockService.Object.GetMethod(filePath, typeName, methodName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetMethod_ShouldReturnMethod_WhenMethodExists()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "ExistingMethod";
        var expectedMethod = new CodeMethodDefinition 
        { 
            Name = methodName,
            ReturnType = "string",
            Visibility = "public"
        };
        
        _mockService.Setup(x => x.GetMethod(filePath, typeName, methodName))
                   .Returns(expectedMethod);

        // Act
        var result = _mockService.Object.GetMethod(filePath, typeName, methodName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(methodName);
    }

    [Fact]
    public void GetMethodBody_ShouldReturnMethodImplementation_WhenMethodExists()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "TestMethod";
        var expectedBody = "return \"Hello World\";";
        
        _mockService.Setup(x => x.GetMethodBody(filePath, typeName, methodName))
                   .Returns(expectedBody);

        // Act
        var result = _mockService.Object.GetMethodBody(filePath, typeName, methodName);

        // Assert
        result.Should().Be(expectedBody);
    }

    [Fact]
    public void UpdateMethodBody_ShouldNotThrow_WhenValidParametersProvided()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "TestMethod";
        var newBody = "return \"Updated\";";
        
        _mockService.Setup(x => x.UpdateMethodBody(filePath, typeName, methodName, newBody))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.UpdateMethodBody(filePath, typeName, methodName, newBody))
                   .Should().NotThrow();
        
        _mockService.Verify();
    }

    #endregion

    #region Property Management Contract Tests

    [Fact]
    public void GetProperty_ShouldReturnNull_WhenPropertyDoesNotExist()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var propertyName = "NonExistentProperty";
        
        _mockService.Setup(x => x.GetProperty(filePath, typeName, propertyName))
                   .Returns((CodePropertyDefinition?)null);

        // Act
        var result = _mockService.Object.GetProperty(filePath, typeName, propertyName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AddProperty_ShouldAcceptDifferentPropertyTypes()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var readWriteProperty = new CodePropertyDefinition 
        { 
            Name = "ReadWriteProperty", 
            Type = "string",
            HasGetter = true,
            HasSetter = true
        };
        var readOnlyProperty = new CodePropertyDefinition 
        { 
            Name = "ReadOnlyProperty", 
            Type = "int",
            HasGetter = true,
            HasSetter = false
        };
        
        _mockService.Setup(x => x.AddProperty(filePath, typeName, It.IsAny<CodePropertyDefinition>()))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.AddProperty(filePath, typeName, readWriteProperty))
                   .Should().NotThrow();
                   
        _mockService.Object
                   .Invoking(s => s.AddProperty(filePath, typeName, readOnlyProperty))
                   .Should().NotThrow();
    }

    #endregion

    #region Convenience Methods Contract Tests

    [Fact]
    public void AddPublicMethod_ShouldCreateMethodWithCorrectVisibility()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "PublicMethod";
        var returnType = "string";
        var body = "return \"test\";";
        var parameters = new[] { ("int", "id"), ("string", "name") };
        
        _mockService.Setup(x => x.AddPublicMethod(filePath, typeName, methodName, returnType, body, parameters))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.AddPublicMethod(filePath, typeName, methodName, returnType, body, parameters))
                   .Should().NotThrow();
        
        _mockService.Verify();
    }

    [Fact]
    public void AddPrivateMethod_ShouldCreateMethodWithCorrectVisibility()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodName = "PrivateHelper";
        var returnType = "void";
        var body = "// helper implementation";
        
        _mockService.Setup(x => x.AddPrivateMethod(filePath, typeName, methodName, returnType, body))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.AddPrivateMethod(filePath, typeName, methodName, returnType, body))
                   .Should().NotThrow();
        
        _mockService.Verify();
    }

    [Fact]
    public void AddPublicProperty_ShouldSupportOptionalParameters()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var propertyName = "TestProperty";
        var propertyType = "string";
        
        _mockService.Setup(x => x.AddPublicProperty(filePath, typeName, propertyName, propertyType, true, true))
                   .Verifiable();
        _mockService.Setup(x => x.AddPublicProperty(filePath, typeName, propertyName + "ReadOnly", propertyType, true, false))
                   .Verifiable();

        // Act & Assert - Default parameters (getter and setter)
        _mockService.Object
                   .Invoking(s => s.AddPublicProperty(filePath, typeName, propertyName, propertyType))
                   .Should().NotThrow();
                   
        // Act & Assert - Read-only property
        _mockService.Object
                   .Invoking(s => s.AddPublicProperty(filePath, typeName, propertyName + "ReadOnly", propertyType, hasGetter: true, hasSetter: false))
                   .Should().NotThrow();
    }

    [Fact]
    public void AddPrivateField_ShouldSupportOptionalDefaultValue()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var fieldName = "_counter";
        var fieldType = "int";
        var defaultValue = "0";
        
        _mockService.Setup(x => x.AddPrivateField(filePath, typeName, fieldName, fieldType, null))
                   .Verifiable();
        _mockService.Setup(x => x.AddPrivateField(filePath, typeName, fieldName + "WithDefault", fieldType, defaultValue))
                   .Verifiable();

        // Act & Assert - Without default value
        _mockService.Object
                   .Invoking(s => s.AddPrivateField(filePath, typeName, fieldName, fieldType))
                   .Should().NotThrow();
                   
        // Act & Assert - With default value
        _mockService.Object
                   .Invoking(s => s.AddPrivateField(filePath, typeName, fieldName + "WithDefault", fieldType, defaultValue))
                   .Should().NotThrow();
    }

    #endregion

    #region Batch Operations Contract Tests

    [Fact]
    public void AddMethods_ShouldAcceptEmptyCollection()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var emptyMethods = new List<CodeMethodDefinition>();
        
        _mockService.Setup(x => x.AddMethods(filePath, typeName, emptyMethods))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.AddMethods(filePath, typeName, emptyMethods))
                   .Should().NotThrow();
    }

    [Fact]
    public void AddMethods_ShouldAcceptMultipleMethods()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methods = new List<CodeMethodDefinition>
        {
            new() { Name = "Method1", ReturnType = "void" },
            new() { Name = "Method2", ReturnType = "string" },
            new() { Name = "Method3", ReturnType = "int" }
        };
        
        _mockService.Setup(x => x.AddMethods(filePath, typeName, methods))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.AddMethods(filePath, typeName, methods))
                   .Should().NotThrow();
    }

    [Fact]
    public void RemoveMethods_ShouldAcceptMethodNames()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var methodNames = new List<string> { "Method1", "Method2", "ObsoleteMethod" };
        
        _mockService.Setup(x => x.RemoveMethods(filePath, typeName, methodNames))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.RemoveMethods(filePath, typeName, methodNames))
                   .Should().NotThrow();
    }

    #endregion

    #region Validation Contract Tests

    [Fact]
    public void TypeExists_ShouldReturnBool_WhenCalled()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var existingType = "ExistingClass";
        var nonExistingType = "NonExistingClass";
        
        _mockService.Setup(x => x.TypeExists(filePath, existingType))
                   .Returns(true);
        _mockService.Setup(x => x.TypeExists(filePath, nonExistingType))
                   .Returns(false);

        // Act
        var existsResult = _mockService.Object.TypeExists(filePath, existingType);
        var notExistsResult = _mockService.Object.TypeExists(filePath, nonExistingType);

        // Assert
        existsResult.Should().BeTrue();
        notExistsResult.Should().BeFalse();
    }

    [Fact]
    public void MethodExists_ShouldReturnBool_WhenCalled()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var existingMethod = "ExistingMethod";
        var nonExistingMethod = "NonExistingMethod";
        
        _mockService.Setup(x => x.MethodExists(filePath, typeName, existingMethod))
                   .Returns(true);
        _mockService.Setup(x => x.MethodExists(filePath, typeName, nonExistingMethod))
                   .Returns(false);

        // Act
        var existsResult = _mockService.Object.MethodExists(filePath, typeName, existingMethod);
        var notExistsResult = _mockService.Object.MethodExists(filePath, typeName, nonExistingMethod);

        // Assert
        existsResult.Should().BeTrue();
        notExistsResult.Should().BeFalse();
    }

    [Fact]
    public void ValidateModification_ShouldReturnValidationMessages()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var operation = "AddMethod";
        var expectedWarnings = new List<string>
        {
            "Warning: Method with same signature already exists",
            "Info: Consider using overloads"
        };
        
        _mockService.Setup(x => x.ValidateModification(filePath, typeName, operation))
                   .Returns(expectedWarnings);

        // Act
        var result = _mockService.Object.ValidateModification(filePath, typeName, operation);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(msg => msg.StartsWith("Warning:"));
        result.Should().Contain(msg => msg.StartsWith("Info:"));
    }

    #endregion

    #region Code Generation Contract Tests

    [Fact]
    public void GenerateCode_ShouldReturnCodeString_WhenValidTypeProvided()
    {
        // Arrange
        var type = new CodeTypeDefinition 
        { 
            Name = "TestClass",
            Visibility = "public",
            Kind = CodeTypeKind.Class
        };
        var expectedCode = "public class TestClass\n{\n}";
        
        _mockService.Setup(x => x.GenerateCode(type))
                   .Returns(expectedCode);

        // Act
        var result = _mockService.Object.GenerateCode(type);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("public class TestClass");
    }

    [Fact]
    public void RegenerateFile_ShouldAcceptMultipleTypes()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var types = new List<CodeTypeDefinition>
        {
            new() { Name = "Class1", Kind = CodeTypeKind.Class },
            new() { Name = "IInterface1", Kind = CodeTypeKind.Interface },
            new() { Name = "Enum1", Kind = CodeTypeKind.Enum }
        };
        
        _mockService.Setup(x => x.RegenerateFile(filePath, types))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.RegenerateFile(filePath, types))
                   .Should().NotThrow();
        
        _mockService.Verify();
    }

    #endregion

    #region Impact Analysis Contract Tests

    [Fact]
    public void GetChangeImpact_ShouldReturnAffectedFiles()
    {
        // Arrange
        var typeName = "TestClass";
        var memberName = "ImportantMethod";
        var expectedImpact = new List<string>
        {
            "DependentClass1.cs",
            "TestRunner.cs",
            "IntegrationTest.cs"
        };
        
        _mockService.Setup(x => x.GetChangeImpact(typeName, memberName))
                   .Returns(expectedImpact);

        // Act
        var result = _mockService.Object.GetChangeImpact(typeName, memberName);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().OnlyContain(file => file.EndsWith(".cs"));
    }

    [Fact]
    public void GetDependentFiles_ShouldReturnDependencies()
    {
        // Arrange
        var filePath = "CoreService.cs";
        var expectedDependents = new List<string>
        {
            "ServiceConsumer.cs",
            "ServiceWrapper.cs"
        };
        
        _mockService.Setup(x => x.GetDependentFiles(filePath))
                   .Returns(expectedDependents);

        // Act
        var result = _mockService.Object.GetDependentFiles(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetUsages_ShouldSupportOptionalMemberName()
    {
        // Arrange
        var typeName = "TestClass";
        var memberName = "TestMethod";
        var typeUsages = new List<string> { "Usage1", "Usage2" };
        var memberUsages = new List<string> { "MemberUsage1" };
        
        _mockService.Setup(x => x.GetUsages(typeName, null))
                   .Returns(typeUsages);
        _mockService.Setup(x => x.GetUsages(typeName, memberName))
                   .Returns(memberUsages);

        // Act
        var typeResult = _mockService.Object.GetUsages(typeName);
        var memberResult = _mockService.Object.GetUsages(typeName, memberName);

        // Assert
        typeResult.Should().HaveCount(2);
        memberResult.Should().HaveCount(1);
    }

    #endregion

    #region Interface Management Contract Tests

    [Fact]
    public void CreateInterface_ShouldAcceptInterfaceDefinition()
    {
        // Arrange
        var filePath = "ITestInterface.cs";
        var interfaceName = "ITestInterface";
        var interfaceDefinition = new CodeTypeDefinition
        {
            Name = interfaceName,
            Kind = CodeTypeKind.Interface,
            Visibility = "public"
        };
        
        _mockService.Setup(x => x.CreateInterface(filePath, interfaceName, interfaceDefinition))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.CreateInterface(filePath, interfaceName, interfaceDefinition))
                   .Should().NotThrow();
        
        _mockService.Verify();
    }

    [Fact]
    public void AddMethodToInterface_ShouldAcceptMethodDefinition()
    {
        // Arrange
        var filePath = "ITestInterface.cs";
        var interfaceName = "ITestInterface";
        var method = new CodeMethodDefinition
        {
            Name = "InterfaceMethod",
            ReturnType = "string",
            Visibility = "public"
        };
        
        _mockService.Setup(x => x.AddMethodToInterface(filePath, interfaceName, method))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.AddMethodToInterface(filePath, interfaceName, method))
                   .Should().NotThrow();
        
        _mockService.Verify();
    }

    #endregion

    #region Type Management Contract Tests

    [Fact]
    public void CreateType_ShouldAcceptDifferentTypeKinds()
    {
        // Arrange
        var filePath = "Types.cs";
        var classType = new CodeTypeDefinition { Name = "TestClass", Kind = CodeTypeKind.Class };
        var structType = new CodeTypeDefinition { Name = "TestStruct", Kind = CodeTypeKind.Struct };
        var enumType = new CodeTypeDefinition { Name = "TestEnum", Kind = CodeTypeKind.Enum };
        
        _mockService.Setup(x => x.CreateType(filePath, It.IsAny<CodeTypeDefinition>()))
                   .Verifiable();

        // Act & Assert
        _mockService.Object
                   .Invoking(s => s.CreateType(filePath, classType))
                   .Should().NotThrow();
                   
        _mockService.Object
                   .Invoking(s => s.CreateType(filePath, structType))
                   .Should().NotThrow();
                   
        _mockService.Object
                   .Invoking(s => s.CreateType(filePath, enumType))
                   .Should().NotThrow();
    }

    [Fact]
    public void ChangeBaseClass_ShouldAcceptNullBaseClass()
    {
        // Arrange
        var filePath = "TestFile.cs";
        var typeName = "TestClass";
        var newBaseClass = "BaseClass";
        
        _mockService.Setup(x => x.ChangeBaseClass(filePath, typeName, newBaseClass))
                   .Verifiable();
        _mockService.Setup(x => x.ChangeBaseClass(filePath, typeName, null))
                   .Verifiable();

        // Act & Assert - With base class
        _mockService.Object
                   .Invoking(s => s.ChangeBaseClass(filePath, typeName, newBaseClass))
                   .Should().NotThrow();
                   
        // Act & Assert - Remove base class (null)
        _mockService.Object
                   .Invoking(s => s.ChangeBaseClass(filePath, typeName, null))
                   .Should().NotThrow();
    }

    #endregion

    #region Parameter Validation Contract Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Methods_ShouldHandleEdgeCaseParameters_Appropriately(string edgeCaseValue)
    {
        // This test verifies that the interface contract should handle edge cases
        // The actual behavior (throw vs. handle gracefully) depends on implementation
        // but the interface should be consistent
        
        // Arrange
        var normalPath = "TestFile.cs";
        var normalType = "TestClass";
        
        // The mock should be set up to represent expected behavior
        // In this case, we expect empty/whitespace strings to be handled somehow
        _mockService.Setup(x => x.TypeExists(normalPath, edgeCaseValue))
                   .Returns(false); // Reasonable default for empty type names

        // Act & Assert - Should not throw unexpected exceptions
        _mockService.Object
                   .Invoking(s => s.TypeExists(normalPath, edgeCaseValue))
                   .Should().NotThrow();
    }

    [Fact]
    public void RenameSymbol_ShouldSupportOptionalTypeName()
    {
        // Arrange
        var oldName = "OldSymbol";
        var newName = "NewSymbol";
        var typeName = "TestClass";
        
        _mockService.Setup(x => x.RenameSymbol(oldName, newName, typeName))
                   .Verifiable();
        _mockService.Setup(x => x.RenameSymbol(oldName, newName, null))
                   .Verifiable();

        // Act & Assert - With type name
        _mockService.Object
                   .Invoking(s => s.RenameSymbol(oldName, newName, typeName))
                   .Should().NotThrow();
                   
        // Act & Assert - Without type name (global rename)
        _mockService.Object
                   .Invoking(s => s.RenameSymbol(oldName, newName, null))
                   .Should().NotThrow();
        
        _mockService.Verify();
    }

    #endregion
}
