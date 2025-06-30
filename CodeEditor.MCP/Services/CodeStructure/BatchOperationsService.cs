using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Service responsible for batch operations on code structures
/// </summary>
public class BatchOperationsService : IBatchOperationsService
{
    private readonly ICodeModificationService _codeModification;

    public BatchOperationsService(ICodeModificationService codeModification)
    {
        _codeModification = codeModification;
    }

    public void AddMethods(string filePath, string typeName, IEnumerable<CodeMethodDefinition> methods)
    {
        foreach (var method in methods)
        {
            _codeModification.AddMethod(filePath, typeName, method);
        }
    }

    public void AddProperties(string filePath, string typeName, IEnumerable<CodePropertyDefinition> properties)
    {
        foreach (var property in properties)
        {
            _codeModification.AddProperty(filePath, typeName, property);
        }
    }

    public void RemoveMethods(string filePath, string typeName, IEnumerable<string> methodNames)
    {
        foreach (var methodName in methodNames)
        {
            _codeModification.RemoveMethod(filePath, typeName, methodName);
        }
    }
}
