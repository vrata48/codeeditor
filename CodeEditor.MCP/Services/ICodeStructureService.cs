using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services;

/// <summary>
/// Service for parsing, analyzing, and manipulating C# code at a structural level
/// </summary>
public interface ICodeStructureService
{
    // Analysis
    CodeTypeDefinition ParseType(string filePath, string typeName);
    List<CodeTypeDefinition> ParseAllTypes(string filePath);
    ProjectStructure AnalyzeProject(string projectPath = ".");
    
    // Querying
    List<CodeTypeDefinition> FindTypesByName(string pattern);
    List<CodeMethodDefinition> FindMethodsBySignature(string returnType, string name, params string[] parameterTypes);
    List<CodeTypeDefinition> FindTypesWithAttribute(string attributeName);
    List<string> FindAllReferences(string typeName, string memberName);
    
    // High-Level Modification Operations
    void ModifyType(CodeTypeDefinition type);
    void RenameSymbol(string oldName, string newName, string? typeName = null);
    
    // Method Management
    void AddMethod(string filePath, string typeName, CodeMethodDefinition method);
    void RemoveMethod(string filePath, string typeName, string methodName);
    void ReplaceMethod(string filePath, string typeName, string oldMethodName, CodeMethodDefinition newMethod);
    CodeMethodDefinition? GetMethod(string filePath, string typeName, string methodName);
    string GetMethodBody(string filePath, string typeName, string methodName);
    void UpdateMethodBody(string filePath, string typeName, string methodName, string newBody);
    
    // Property Management
    void AddProperty(string filePath, string typeName, CodePropertyDefinition property);
    void RemoveProperty(string filePath, string typeName, string propertyName);
    void ReplaceProperty(string filePath, string typeName, string oldPropertyName, CodePropertyDefinition newProperty);
    CodePropertyDefinition? GetProperty(string filePath, string typeName, string propertyName);
    
    // Field Management
    void AddField(string filePath, string typeName, CodeFieldDefinition field);
    void RemoveField(string filePath, string typeName, string fieldName);
    void ReplaceField(string filePath, string typeName, string oldFieldName, CodeFieldDefinition newField);
    
    // Interface Management
    void CreateInterface(string filePath, string interfaceName, CodeTypeDefinition interfaceDefinition);
    void AddMethodToInterface(string filePath, string interfaceName, CodeMethodDefinition method);
    void RemoveMethodFromInterface(string filePath, string interfaceName, string methodName);
    void AddPropertyToInterface(string filePath, string interfaceName, CodePropertyDefinition property);
    
    // Type Management
    void CreateType(string filePath, CodeTypeDefinition type);
    void RemoveType(string filePath, string typeName);
    void AddInterface(string filePath, string typeName, string interfaceName);
    void RemoveInterface(string filePath, string typeName, string interfaceName);
    void ChangeBaseClass(string filePath, string typeName, string? newBaseClass);
    
    // Convenience Methods
    void AddPublicMethod(string filePath, string typeName, string methodName, string returnType, 
        string body, params (string type, string name)[] parameters);
    void AddPrivateMethod(string filePath, string typeName, string methodName, string returnType, 
        string body, params (string type, string name)[] parameters);
    void AddPublicProperty(string filePath, string typeName, string propertyName, string type, 
        bool hasGetter = true, bool hasSetter = true);
    void AddPrivateField(string filePath, string typeName, string fieldName, string type, 
        string? defaultValue = null);
    
    // Batch Operations
    void AddMethods(string filePath, string typeName, IEnumerable<CodeMethodDefinition> methods);
    void AddProperties(string filePath, string typeName, IEnumerable<CodePropertyDefinition> properties);
    void RemoveMethods(string filePath, string typeName, IEnumerable<string> methodNames);
    
    // Validation
    bool TypeExists(string filePath, string typeName);
    bool MethodExists(string filePath, string typeName, string methodName);
    bool PropertyExists(string filePath, string typeName, string propertyName);
    List<string> ValidateModification(string filePath, string typeName, string operation);
    
    // Code Generation
    string GenerateCode(CodeTypeDefinition type);
    void RegenerateFile(string filePath, List<CodeTypeDefinition> types);
    
    // Impact Analysis
    List<string> GetChangeImpact(string typeName, string memberName);
    List<string> GetDependentFiles(string filePath);
    List<string> GetUsages(string typeName, string? memberName = null);
}
