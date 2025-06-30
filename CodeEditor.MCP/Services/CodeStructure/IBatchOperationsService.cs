using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Interface for batch operations on code structures
/// </summary>
public interface IBatchOperationsService
{
    void AddMethods(string filePath, string typeName, IEnumerable<CodeMethodDefinition> methods);
    void AddProperties(string filePath, string typeName, IEnumerable<CodePropertyDefinition> properties);
    void RemoveMethods(string filePath, string typeName, IEnumerable<string> methodNames);
}
