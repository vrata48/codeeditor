using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Service responsible for code refactoring operations
/// </summary>
public class CodeRefactoringService : ICodeRefactoringService
{
    private readonly ICodeStructureCache _cache;
    private readonly ICodeModificationService _codeModification;

    public CodeRefactoringService(
        ICodeStructureCache cache,
        ICodeModificationService codeModification)
    {
        _cache = cache;
        _codeModification = codeModification;
    }

    public void RenameSymbol(string oldName, string newName, string? typeName = null)
    {
        if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            throw new ArgumentException("Old name and new name cannot be null or empty");

        if (oldName == newName)
            return; // Nothing to do

        var affectedTypes = new List<CodeTypeDefinition>();

        // Find all types that need to be updated
        foreach (var (key, type) in _cache.GetAllCachedTypes())
        {
            bool needsUpdate = false;

            // If we're renaming a specific type
            if (typeName == null && type.Name == oldName)
            {
                type.Name = newName;
                needsUpdate = true;
            }
            // If we're renaming a member within a specific type
            else if (typeName != null && type.Name == typeName)
            {
                needsUpdate = RenameSymbolInType(type, oldName, newName);
            }
            // Update references in other types
            else
            {
                needsUpdate = UpdateReferencesInType(type, oldName, newName, typeName);
            }

            if (needsUpdate)
            {
                affectedTypes.Add(type);
            }
        }

        // Apply changes to files
        var fileGroups = affectedTypes.GroupBy(t => t.FilePath);
        foreach (var fileGroup in fileGroups)
        {
            var filePath = fileGroup.Key;
            var typesInFile = fileGroup.ToList();
            _codeModification.RegenerateFile(filePath, typesInFile);
        }
    }

    private bool RenameSymbolInType(CodeTypeDefinition type, string oldName, string newName)
    {
        bool hasChanges = false;

        // Rename methods
        var methodToRename = type.Members.Methods.FirstOrDefault(m => m.Name == oldName);
        if (methodToRename != null)
        {
            methodToRename.Name = newName;
            hasChanges = true;
        }

        // Rename properties
        var propertyToRename = type.Members.Properties.FirstOrDefault(p => p.Name == oldName);
        if (propertyToRename != null)
        {
            propertyToRename.Name = newName;
            hasChanges = true;
        }

        // Rename fields
        var fieldToRename = type.Members.Fields.FirstOrDefault(f => f.Name == oldName);
        if (fieldToRename != null)
        {
            fieldToRename.Name = newName;
            hasChanges = true;
        }

        // Update references within the same type
        hasChanges |= UpdateReferencesInType(type, oldName, newName, null);

        return hasChanges;
    }

    private bool UpdateReferencesInType(CodeTypeDefinition type, string oldName, string newName, string? typeName)
    {
        bool hasChanges = false;

        // Update base type reference
        if (type.BaseType == oldName)
        {
            type.BaseType = newName;
            hasChanges = true;
        }

        // Update interface references
        for (int i = 0; i < type.Interfaces.Count; i++)
        {
            if (type.Interfaces[i] == oldName)
            {
                type.Interfaces[i] = newName;
                hasChanges = true;
            }
        }

        // Update method signatures and bodies
        foreach (var method in type.Members.Methods)
        {
            // Update return type
            if (method.ReturnType.Contains(oldName))
            {
                method.ReturnType = method.ReturnType.Replace(oldName, newName);
                hasChanges = true;
            }

            // Update parameter types
            foreach (var parameter in method.Parameters)
            {
                if (parameter.Type.Contains(oldName))
                {
                    parameter.Type = parameter.Type.Replace(oldName, newName);
                    hasChanges = true;
                }
            }

            // Update method body
            if (method.Body != null)
            {
                string updatedBody;
                if (typeName != null)
                {
                    // Replace specific member references
                    updatedBody = method.Body
                        .Replace($"{typeName}.{oldName}", $"{typeName}.{newName}")
                        .Replace($".{oldName}(", $".{newName}(")
                        .Replace($".{oldName} ", $".{newName} ");
                }
                else
                {
                    // Replace type references
                    updatedBody = method.Body.Replace(oldName, newName);
                }

                if (updatedBody != method.Body)
                {
                    method.Body = updatedBody;
                    hasChanges = true;
                }
            }
        }

        // Update property types
        foreach (var property in type.Members.Properties)
        {
            if (property.Type.Contains(oldName))
            {
                property.Type = property.Type.Replace(oldName, newName);
                hasChanges = true;
            }
        }

        // Update field types
        foreach (var field in type.Members.Fields)
        {
            if (field.Type.Contains(oldName))
            {
                field.Type = field.Type.Replace(oldName, newName);
                hasChanges = true;
            }
        }

        // Update event types
        foreach (var eventDef in type.Members.Events)
        {
            if (eventDef.Type.Contains(oldName))
            {
                eventDef.Type = eventDef.Type.Replace(oldName, newName);
                hasChanges = true;
            }
        }

        return hasChanges;
    }
}
