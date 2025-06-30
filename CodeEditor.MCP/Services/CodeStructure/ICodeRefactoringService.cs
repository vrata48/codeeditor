namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Interface for code refactoring operations
/// </summary>
public interface ICodeRefactoringService
{
    void RenameSymbol(string oldName, string newName, string? typeName = null);
}
