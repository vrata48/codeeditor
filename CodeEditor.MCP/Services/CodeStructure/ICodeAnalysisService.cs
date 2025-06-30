using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Interface for analyzing and parsing C# code structures
/// </summary>
public interface ICodeAnalysisService
{
    CodeTypeDefinition ParseType(string filePath, string typeName);
    List<CodeTypeDefinition> ParseAllTypes(string filePath);
    ProjectStructure AnalyzeProject(string projectPath = ".");
}
