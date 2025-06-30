using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Interface for modifying existing code structures
/// </summary>
public interface ICodeModificationService
{
    void AddMethod(string filePath, string typeName, CodeMethodDefinition method);
    void RemoveMethod(string filePath, string typeName, string methodName);
    void ReplaceMethod(string filePath, string typeName, string oldMethodName, CodeMethodDefinition newMethod);
    void UpdateMethodBody(string filePath, string typeName, string methodName, string newBody);
    
    void AddProperty(string filePath, string typeName, CodePropertyDefinition property);
    void RemoveProperty(string filePath, string typeName, string propertyName);
    void ReplaceProperty(string filePath, string typeName, string oldPropertyName, CodePropertyDefinition newProperty);
    
    void AddField(string filePath, string typeName, CodeFieldDefinition field);
    void RemoveField(string filePath, string typeName, string fieldName);
    void ReplaceField(string filePath, string typeName, string oldFieldName, CodeFieldDefinition newField);
    
    void CreateType(string filePath, CodeTypeDefinition type);
    void RemoveType(string filePath, string typeName);
    void ModifyType(CodeTypeDefinition type);
    
    void AddInterface(string filePath, string typeName, string interfaceName);
    void RemoveInterface(string filePath, string typeName, string interfaceName);
    void ChangeBaseClass(string filePath, string typeName, string? newBaseClass);
    
    void RegenerateFile(string filePath, List<CodeTypeDefinition> types);
}
