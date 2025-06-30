namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Interface for validating code structure operations
/// </summary>
public interface ICodeValidationService
{
    bool TypeExists(string filePath, string typeName);
    bool MethodExists(string filePath, string typeName, string methodName);
    bool PropertyExists(string filePath, string typeName, string propertyName);
    List<string> ValidateModification(string filePath, string typeName, string operation);
}
