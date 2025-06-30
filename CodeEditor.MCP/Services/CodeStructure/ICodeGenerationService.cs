using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Interface for generating C# code from structures
/// </summary>
public interface ICodeGenerationService
{
    string GenerateCode(CodeTypeDefinition type);
}
