using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeEditor.MCP.Models;
using System.IO.Abstractions;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Service responsible for modifying existing code structures
/// </summary>
public class CodeModificationService : ICodeModificationService
{
    private readonly IFileSystem _fileSystem;
    private readonly IPathService _pathService;
    private readonly ICodeStructureCache _cache;
    private readonly ICodeGenerationService _codeGeneration;
    private readonly ICodeAnalysisService _codeAnalysis;

    public CodeModificationService(
        IFileSystem fileSystem, 
        IPathService pathService, 
        ICodeStructureCache cache,
        ICodeGenerationService codeGeneration,
        ICodeAnalysisService codeAnalysis)
    {
        _fileSystem = fileSystem;
        _pathService = pathService;
        _cache = cache;
        _codeGeneration = codeGeneration;
        _codeAnalysis = codeAnalysis;
    }

    public void AddMethod(string filePath, string typeName, CodeMethodDefinition method)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            if (type.Members.Methods.Any(m => m.Name == method.Name))
                throw new InvalidOperationException($"Method '{method.Name}' already exists in type '{typeName}'");
                
            type.Members.Methods.Add(method);
        });
    }

    public void RemoveMethod(string filePath, string typeName, string methodName)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            var method = type.Members.Methods.FirstOrDefault(m => m.Name == methodName);
            if (method == null)
                throw new InvalidOperationException($"Method '{methodName}' not found in type '{typeName}'");
                
            type.Members.Methods.Remove(method);
        });
    }

    public void ReplaceMethod(string filePath, string typeName, string oldMethodName, CodeMethodDefinition newMethod)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            var oldMethod = type.Members.Methods.FirstOrDefault(m => m.Name == oldMethodName);
            if (oldMethod == null)
                throw new InvalidOperationException($"Method '{oldMethodName}' not found in type '{typeName}'");
                
            var index = type.Members.Methods.IndexOf(oldMethod);
            type.Members.Methods[index] = newMethod;
        });
    }

    public void UpdateMethodBody(string filePath, string typeName, string methodName, string newBody)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            var method = type.Members.Methods.FirstOrDefault(m => m.Name == methodName);
            if (method == null)
                throw new InvalidOperationException($"Method '{methodName}' not found in type '{typeName}'");
                
            method.Body = newBody;
        });
    }

    public void AddProperty(string filePath, string typeName, CodePropertyDefinition property)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            if (type.Members.Properties.Any(p => p.Name == property.Name))
                throw new InvalidOperationException($"Property '{property.Name}' already exists in type '{typeName}'");
                
            type.Members.Properties.Add(property);
        });
    }

    public void RemoveProperty(string filePath, string typeName, string propertyName)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            var property = type.Members.Properties.FirstOrDefault(p => p.Name == propertyName);
            if (property == null)
                throw new InvalidOperationException($"Property '{propertyName}' not found in type '{typeName}'");
                
            type.Members.Properties.Remove(property);
        });
    }

    public void ReplaceProperty(string filePath, string typeName, string oldPropertyName, CodePropertyDefinition newProperty)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            var oldProperty = type.Members.Properties.FirstOrDefault(p => p.Name == oldPropertyName);
            if (oldProperty == null)
                throw new InvalidOperationException($"Property '{oldPropertyName}' not found in type '{typeName}'");
                
            var index = type.Members.Properties.IndexOf(oldProperty);
            type.Members.Properties[index] = newProperty;
        });
    }

    public void AddField(string filePath, string typeName, CodeFieldDefinition field)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            if (type.Members.Fields.Any(f => f.Name == field.Name))
                throw new InvalidOperationException($"Field '{field.Name}' already exists in type '{typeName}'");
                
            type.Members.Fields.Add(field);
        });
    }

    public void RemoveField(string filePath, string typeName, string fieldName)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            var field = type.Members.Fields.FirstOrDefault(f => f.Name == fieldName);
            if (field == null)
                throw new InvalidOperationException($"Field '{fieldName}' not found in type '{typeName}'");
                
            type.Members.Fields.Remove(field);
        });
    }

    public void ReplaceField(string filePath, string typeName, string oldFieldName, CodeFieldDefinition newField)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            var oldField = type.Members.Fields.FirstOrDefault(f => f.Name == oldFieldName);
            if (oldField == null)
                throw new InvalidOperationException($"Field '{oldFieldName}' not found in type '{typeName}'");
                
            var index = type.Members.Fields.IndexOf(oldField);
            type.Members.Fields[index] = newField;
        });
    }

    public void CreateType(string filePath, CodeTypeDefinition type)
    {
        var fullPath = _pathService.GetFullPath(filePath);
        
        if (_fileSystem.File.Exists(fullPath))
        {
            // Add to existing file
            var existingTypes = _codeAnalysis.ParseAllTypes(filePath);
            if (existingTypes.Any(t => t.Name == type.Name))
                throw new InvalidOperationException($"Type '{type.Name}' already exists in file '{filePath}'");
                
            existingTypes.Add(type);
            RegenerateFile(filePath, existingTypes);
        }
        else
        {
            // Create new file
            var code = _codeGeneration.GenerateCode(type);
            _fileSystem.File.WriteAllText(fullPath, code);
        }
        
        _cache.InvalidateFile(filePath);
    }

    public void RemoveType(string filePath, string typeName)
    {
        var types = _codeAnalysis.ParseAllTypes(filePath);
        var typeToRemove = types.FirstOrDefault(t => t.Name == typeName);
        
        if (typeToRemove == null)
            throw new InvalidOperationException($"Type '{typeName}' not found in file '{filePath}'");
            
        types.Remove(typeToRemove);
        RegenerateFile(filePath, types);
    }

    public void ModifyType(CodeTypeDefinition type)
    {
        var filePath = type.FilePath;
        ModifyTypeInFile(filePath, type.Name, existingType =>
        {
            // Update all properties from the modified type
            existingType.Visibility = type.Visibility;
            existingType.IsStatic = type.IsStatic;
            existingType.IsAbstract = type.IsAbstract;
            existingType.IsSealed = type.IsSealed;
            existingType.IsPartial = type.IsPartial;
            existingType.BaseType = type.BaseType;
            existingType.Interfaces = type.Interfaces;
            existingType.Members = type.Members;
        });
    }

    public void AddInterface(string filePath, string typeName, string interfaceName)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            if (type.Interfaces.Contains(interfaceName))
                throw new InvalidOperationException($"Interface '{interfaceName}' is already implemented by type '{typeName}'");
                
            type.Interfaces.Add(interfaceName);
        });
    }

    public void RemoveInterface(string filePath, string typeName, string interfaceName)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            if (!type.Interfaces.Contains(interfaceName))
                throw new InvalidOperationException($"Interface '{interfaceName}' is not implemented by type '{typeName}'");
                
            type.Interfaces.Remove(interfaceName);
        });
    }

    public void ChangeBaseClass(string filePath, string typeName, string? newBaseClass)
    {
        ModifyTypeInFile(filePath, typeName, type =>
        {
            type.BaseType = newBaseClass;
        });
    }

    public void RegenerateFile(string filePath, List<CodeTypeDefinition> types)
    {
        var fullPath = _pathService.GetFullPath(filePath);
        var codeBlocks = types.Select(type => _codeGeneration.GenerateCode(type));
        var fileContent = string.Join("\n\n", codeBlocks);
        
        _fileSystem.File.WriteAllText(fullPath, fileContent);
        _cache.InvalidateFile(filePath);
    }

    private void ModifyTypeInFile(string filePath, string typeName, Action<CodeTypeDefinition> modification)
    {
        var types = _codeAnalysis.ParseAllTypes(filePath);
        var targetType = types.FirstOrDefault(t => t.Name == typeName);
        
        if (targetType == null)
            throw new InvalidOperationException($"Type '{typeName}' not found in file '{filePath}'");
            
        modification(targetType);
        RegenerateFile(filePath, types);
    }
}
