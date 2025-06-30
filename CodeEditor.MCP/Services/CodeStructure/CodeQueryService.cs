using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Service responsible for searching and querying code structures
/// </summary>
public class CodeQueryService : ICodeQueryService
{
    private readonly ICodeStructureCache _cache;

    public CodeQueryService(ICodeStructureCache cache)
    {
        _cache = cache;
    }

    public List<CodeTypeDefinition> FindTypesByName(string pattern)
    {
        var results = new List<CodeTypeDefinition>();

        foreach (var (key, type) in _cache.GetAllCachedTypes())
        {
            if (IsMatch(type.Name, pattern))
            {
                results.Add(type);
            }
        }

        return results;
    }

    public List<CodeMethodDefinition> FindMethodsBySignature(string returnType, string name,
        params string[] parameterTypes)
    {
        var results = new List<CodeMethodDefinition>();

        foreach (var (key, type) in _cache.GetAllCachedTypes())
        {
            foreach (var method in type.Members.Methods)
            {
                if (DoesMethodMatchSignature(method, returnType, name, parameterTypes))
                {
                    results.Add(method);
                }
            }
        }

        return results;
    }

    public List<string> FindAllReferences(string typeName, string memberName)
    {
        var references = new List<string>();

        foreach (var (key, type) in _cache.GetAllCachedTypes())
        {
            // Check if this type references the target type
            if (ReferencesType(type, typeName))
            {
                references.Add($"Type: {type.Name} in {type.FilePath}");
            }

            // Check if any members reference the target member
            foreach (var method in type.Members.Methods)
            {
                if (method.Body?.Contains($"{typeName}.{memberName}") == true ||
                    method.Body?.Contains(memberName) == true)
                {
                    references.Add($"Method: {type.Name}.{method.Name} in {type.FilePath}");
                }
            }

            foreach (var property in type.Members.Properties)
            {
                if (property.Type.Contains(typeName))
                {
                    references.Add($"Property: {type.Name}.{property.Name} in {type.FilePath}");
                }
            }

            foreach (var field in type.Members.Fields)
            {
                if (field.Type.Contains(typeName))
                {
                    references.Add($"Field: {type.Name}.{field.Name} in {type.FilePath}");
                }
            }
        }

        return references.Distinct().ToList();
    }

    public List<string> GetChangeImpact(string typeName, string memberName)
    {
        var impact = new List<string>();
        var directReferences = FindAllReferences(typeName, memberName);

        impact.AddRange(directReferences);

        // Find transitive dependencies
        foreach (var reference in directReferences)
        {
            var parts = reference.Split(':');
            if (parts.Length >= 2)
            {
                var referencingType = parts[1].Trim().Split('.')[0];
                var transitiveRefs = FindAllReferences(referencingType, "");
                impact.AddRange(transitiveRefs.Where(t => !impact.Contains(t)));
            }
        }

        return impact.Distinct().ToList();
    }

    public List<string> GetDependentFiles(string filePath)
    {
        var dependentFiles = new HashSet<string>();

        // Get all types in the target file
        var typesInFile = new List<CodeTypeDefinition>();
        foreach (var (key, type) in _cache.GetAllCachedTypes())
        {
            if (type.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
            {
                typesInFile.Add(type);
            }
        }

        // Find files that reference any of these types
        foreach (var targetType in typesInFile)
        {
            foreach (var (key, type) in _cache.GetAllCachedTypes())
            {
                if (type.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                    continue; // Skip the same file

                if (ReferencesType(type, targetType.Name))
                {
                    dependentFiles.Add(type.FilePath);
                }
            }
        }

        return dependentFiles.ToList();
    }

    public List<string> GetUsages(string typeName, string? memberName = null)
    {
        var usages = new List<string>();

        foreach (var (key, type) in _cache.GetAllCachedTypes())
        {
            // Check base class and interfaces
            if (type.BaseType == typeName || type.Interfaces.Contains(typeName))
            {
                usages.Add($"Inheritance: {type.Name} in {type.FilePath}");
            }

            // Check method parameters and return types
            foreach (var method in type.Members.Methods)
            {
                if (method.ReturnType.Contains(typeName))
                {
                    usages.Add($"Method return: {type.Name}.{method.Name} in {type.FilePath}");
                }

                foreach (var param in method.Parameters)
                {
                    if (param.Type.Contains(typeName))
                    {
                        usages.Add($"Method parameter: {type.Name}.{method.Name} in {type.FilePath}");
                    }
                }

                if (memberName != null && method.Body?.Contains($"{typeName}.{memberName}") == true)
                {
                    usages.Add($"Method body: {type.Name}.{method.Name} in {type.FilePath}");
                }
            }

            // Check properties
            foreach (var property in type.Members.Properties)
            {
                if (property.Type.Contains(typeName))
                {
                    usages.Add($"Property: {type.Name}.{property.Name} in {type.FilePath}");
                }
            }

            // Check fields
            foreach (var field in type.Members.Fields)
            {
                if (field.Type.Contains(typeName))
                {
                    usages.Add($"Field: {type.Name}.{field.Name} in {type.FilePath}");
                }
            }
        }

        return usages.Distinct().ToList();
    }

    private bool IsMatch(string name, string pattern)
    {
        // Simple pattern matching - can be enhanced with regex if needed
        if (pattern.Contains("*"))
        {
            var parts = pattern.Split('*');
            var current = name;

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                var index = current.IndexOf(part, StringComparison.OrdinalIgnoreCase);
                if (index == -1) return false;

                current = current.Substring(index + part.Length);
            }

            return true;
        }

        return name.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private bool DoesMethodMatchSignature(CodeMethodDefinition method, string returnType, string name,
        string[] parameterTypes)
    {
        if (!IsMatch(method.Name, name))
            return false;

        if (!string.IsNullOrEmpty(returnType) && !IsMatch(method.ReturnType, returnType))
            return false;

        if (parameterTypes.Length > 0)
        {
            if (method.Parameters.Count != parameterTypes.Length)
                return false;

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (!IsMatch(method.Parameters[i].Type, parameterTypes[i]))
                    return false;
            }
        }

        return true;
    }

    private bool ReferencesType(CodeTypeDefinition type, string targetTypeName)
    {
        // Check inheritance
        if (type.BaseType == targetTypeName || type.Interfaces.Contains(targetTypeName))
            return true;

        // Check members
        foreach (var method in type.Members.Methods)
        {
            if (method.ReturnType.Contains(targetTypeName))
                return true;

            if (method.Parameters.Any(p => p.Type.Contains(targetTypeName)))
                return true;

            if (method.Body?.Contains(targetTypeName) == true)
                return true;
        }

        foreach (var property in type.Members.Properties)
        {
            if (property.Type.Contains(targetTypeName))
                return true;
        }

        foreach (var field in type.Members.Fields)
        {
            if (field.Type.Contains(targetTypeName))
                return true;
        }

        return false;
    }
    
    public List<CodeTypeDefinition> FindTypesWithAttribute(string attributeName)
    {
        var results = new List<CodeTypeDefinition>();

        foreach (var (key, type) in _cache.GetAllCachedTypes())
        {
            if (type.Attributes.Any(attr => attr.Contains(attributeName)))
            {
                results.Add(type);
            }
        }

        return results;
    }
}
