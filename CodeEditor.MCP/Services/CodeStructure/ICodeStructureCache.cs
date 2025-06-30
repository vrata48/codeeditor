using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Interface for caching code structure information
/// </summary>
public interface ICodeStructureCache
{
    bool TryGetType(string filePath, string typeName, out CodeTypeDefinition? typeDefinition);
    void CacheType(string filePath, CodeTypeDefinition typeDefinition);
    void InvalidateFile(string filePath);
    void Clear();
    IEnumerable<(string Key, CodeTypeDefinition Type)> GetAllCachedTypes();
}
