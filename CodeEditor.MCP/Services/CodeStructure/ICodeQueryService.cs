using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Interface for querying code structures
/// </summary>
public interface ICodeQueryService
{
    List<CodeTypeDefinition> FindTypesByName(string pattern);
    List<CodeMethodDefinition> FindMethodsBySignature(string returnType, string name, params string[] parameterTypes);
    List<CodeTypeDefinition> FindTypesWithAttribute(string attributeName);
    List<string> FindAllReferences(string typeName, string memberName);
    List<string> GetChangeImpact(string typeName, string memberName);
    List<string> GetDependentFiles(string filePath);
    List<string> GetUsages(string typeName, string? memberName = null);
}
