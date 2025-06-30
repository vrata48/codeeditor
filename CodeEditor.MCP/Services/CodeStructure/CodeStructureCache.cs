using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Shared caching service for parsed types across all code structure services
/// </summary>
public class CodeStructureCache : ICodeStructureCache
{
    private readonly Dictionary<string, CodeTypeDefinition> _typeCache = new();

    public bool TryGetType(string filePath, string typeName, out CodeTypeDefinition? type)
    {
        var key = $"{filePath}:{typeName}";
        return _typeCache.TryGetValue(key, out type);
    }

    public void CacheType(string filePath, CodeTypeDefinition type)
    {
        var key = $"{filePath}:{type.Name}";
        _typeCache[key] = type;
    }

    public void InvalidateFile(string filePath)
    {
        var keysToRemove = _typeCache.Keys.Where(k => k.StartsWith($"{filePath}:")).ToList();
        foreach (var key in keysToRemove)
        {
            _typeCache.Remove(key);
        }
    }

    public void Clear()
    {
        _typeCache.Clear();
    }

    // Legacy methods for backward compatibility
    public void CacheType(string filePath, string typeName, CodeTypeDefinition type)
    {
        var key = $"{filePath}:{typeName}";
        _typeCache[key] = type;
    }

    public void InvalidateType(string filePath, string typeName)
    {
        var key = $"{filePath}:{typeName}";
        _typeCache.Remove(key);
    }

    public IEnumerable<(string Key, CodeTypeDefinition Type)> GetAllCachedTypes()
    {
        return _typeCache.Select(kvp => (kvp.Key, kvp.Value));
    }
}
