using System.ComponentModel;
using CodeEditor.MCP.Models;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Aspects;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
public static class CSharpTools
{
    [McpServerTool]
    [Description("Analyze C# file for classes and methods")]
    public static CodeTypeDefinition[] AnalyzeFile(ICodeStructureService service, [Description("Relative path to .cs file")] string path)
    {
        var types = service.ParseAllTypes(path);
        
        // Create copies of type definitions without method names
        var anonymizedTypes = types.Select(type => 
        {
            var anonymizedType = new CodeTypeDefinition
            {
                Name = type.Name,
                Namespace = type.Namespace,
                FilePath = type.FilePath,
                Kind = type.Kind,
                Visibility = type.Visibility,
                IsStatic = type.IsStatic,
                IsAbstract = type.IsAbstract,
                IsSealed = type.IsSealed,
                IsPartial = type.IsPartial,
                BaseType = type.BaseType,
                Interfaces = type.Interfaces,
                Usings = type.Usings,
                Attributes = type.Attributes,
                Documentation = type.Documentation,
                StartLine = type.StartLine,
                EndLine = type.EndLine
            };
            
            // Copy members but anonymize method names
            anonymizedType.Members.Methods = type.Members.Methods.Select(method => new CodeMethodDefinition
            {
                Name = "", // Remove method name
                Visibility = method.Visibility,
                ReturnType = method.ReturnType,
                Parameters = method.Parameters,
                IsStatic = method.IsStatic,
                IsVirtual = method.IsVirtual,
                IsOverride = method.IsOverride,
                IsAbstract = method.IsAbstract,
                IsAsync = method.IsAsync,
                Attributes = method.Attributes,
                StartLine = method.StartLine,
                EndLine = method.EndLine,
                Body = method.Body,
                Documentation = method.Documentation
            }).ToList();
            
            // Copy other members as-is (properties, fields, events)
            anonymizedType.Members.Properties = type.Members.Properties;
            anonymizedType.Members.Fields = type.Members.Fields;
            anonymizedType.Members.Events = type.Members.Events;
            
            return anonymizedType;
        }).ToArray();
        
        return anonymizedTypes;
    }

    [McpServerTool]
    [Description("Read member from class/interface (method body, property definition, field definition)")]
    public static string ReadMember(
        ICodeStructureService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class or interface")] string typeName,
        [Description("Member type: 'method', 'property', 'field'")] string memberType,
        [Description("Name of the member to read")] string memberName)
    {
        var memberTypeLower = memberType.ToLower();
        switch (memberTypeLower)
        {
            case "method":
                return service.GetMethodBody(path, typeName, memberName);
            case "property":
                var property = service.GetProperty(path, typeName, memberName);
                return property != null ? JsonSerializer.Serialize(property, new JsonSerializerOptions { WriteIndented = true }) : "Property not found";
            case "field":
                // For fields, we need to get the type and find the field
                var type = service.ParseType(path, typeName);
                var field = type.Members.Fields.FirstOrDefault(f => f.Name == memberName);
                return field != null ? JsonSerializer.Serialize(field, new JsonSerializerOptions { WriteIndented = true }) : "Field not found";
            default:
                throw new ArgumentException($"Unknown member type: {memberType}. Valid types: method, property, field");
        }
    }

    [McpServerTool]
    [Description("Add member to class/interface (method, property, field)")]
    public static void AddMember(
        ICodeStructureService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class or interface")] string typeName,
        [Description("Member type: 'method', 'property', 'field'")] string memberType,
        [Description("Name of the member")] string memberName,
        [Description("Complete member code")] string memberCode)
    {
        var memberTypeLower = memberType.ToLower();
        switch (memberTypeLower)
        {
            case "method":
                var method = ParseMethodCode(memberName, memberCode);
                service.AddMethod(path, typeName, method);
                break;
            case "property":
                var property = ParsePropertyCode(memberName, memberCode);
                service.AddProperty(path, typeName, property);
                break;
            case "field":
                var field = ParseFieldCode(memberName, memberCode);
                service.AddField(path, typeName, field);
                break;
            default:
                throw new ArgumentException($"Unknown member type: {memberType}. Valid types: method, property, field");
        }
    }

    [McpServerTool]
    [Description("Remove member from class/interface (method, property, field)")]
    public static void RemoveMember(
        ICodeStructureService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class or interface")] string typeName,
        [Description("Member type: 'method', 'property', 'field'")] string memberType,
        [Description("Name of the member to remove")] string memberName)
    {
        var memberTypeLower = memberType.ToLower();
        switch (memberTypeLower)
        {
            case "method":
                service.RemoveMethod(path, typeName, memberName);
                break;
            case "property":
                service.RemoveProperty(path, typeName, memberName);
                break;
            case "field":
                service.RemoveField(path, typeName, memberName);
                break;
            default:
                throw new ArgumentException($"Unknown member type: {memberType}. Valid types: method, property, field");
        }
    }

    [McpServerTool]
    [Description("Replace existing member in class/interface (method, property, field)")]
    public static void ReplaceMember(
        ICodeStructureService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class or interface")] string typeName,
        [Description("Member type: 'method', 'property', 'field'")] string memberType,
        [Description("Name of existing member to replace")] string existingMemberName,
        [Description("Name of new member")] string newMemberName,
        [Description("Complete new member code")] string newMemberCode)
    {
        var memberTypeLower = memberType.ToLower();
        switch (memberTypeLower)
        {
            case "method":
                var method = ParseMethodCode(newMemberName, newMemberCode);
                service.ReplaceMethod(path, typeName, existingMemberName, method);
                break;
            case "property":
                var property = ParsePropertyCode(newMemberName, newMemberCode);
                service.ReplaceProperty(path, typeName, existingMemberName, property);
                break;
            case "field":
                var field = ParseFieldCode(newMemberName, newMemberCode);
                service.ReplaceField(path, typeName, existingMemberName, field);
                break;
            default:
                throw new ArgumentException($"Unknown member type: {memberType}. Valid types: method, property, field");
        }
    }

    [McpServerTool]
    [Description("Create new C# type (class, interface, struct, enum)")]
    public static void CreateType(
        ICodeStructureService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the type")] string typeName,
        [Description("Type kind: 'class', 'interface', 'struct', 'enum'")] string typeKind,
        [Description("Complete type code")] string typeCode)
    {
        var type = ParseTypeCode(typeName, typeKind, typeCode);
        type.FilePath = path; // Set the file path
        
        // Use appropriate service method based on type kind
        if (typeKind.ToLower() == "interface")
        {
            service.CreateInterface(path, typeName, type);
        }
        else
        {
            service.CreateType(path, type);
        }
    }

    [McpServerTool]
    [Description("Formats a C# document using Roslyn formatting rules")]
    public static string FormatDocument(
        ICSharpFormattingService formattingService,
        [Description("Relative path to the .cs file")] string path)
    {
        return formattingService.FormatDocument(path);
    }

    [McpServerTool]
    [Description("Formats multiple C# documents in a directory")]
    public static string FormatDirectory(
        ICSharpFormattingService formattingService,
        [Description("Relative path to directory containing .cs files")] string path,
        [Description("Whether to format files in subdirectories")] bool recursive = false)
    {
        return formattingService.FormatDirectory(path, recursive);
    }

    #region Helper Methods

    private static CodeMethodDefinition ParseMethodCode(string methodName, string methodCode)
    {
        // Simple parsing - extract basic information from method signature
        var lines = methodCode.Split('\n');
        var signatureLine = lines.FirstOrDefault(l => l.Trim().Contains(methodName))?.Trim() ?? methodCode.Trim();
        
        var visibility = "public";
        if (signatureLine.Contains("private ")) visibility = "private";
        else if (signatureLine.Contains("protected ")) visibility = "protected";
        else if (signatureLine.Contains("internal ")) visibility = "internal";
        
        var isStatic = signatureLine.Contains("static ");
        var isAsync = signatureLine.Contains("async ");
        var isVirtual = signatureLine.Contains("virtual ");
        var isOverride = signatureLine.Contains("override ");
        var isAbstract = signatureLine.Contains("abstract ");
        
        var returnType = "void";
        // Try to extract return type - look for pattern after async/static/etc and before method name
        var parts = signatureLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i + 1].Contains(methodName))
            {
                returnType = parts[i];
                break;
            }
        }
        
        return new CodeMethodDefinition
        {
            Name = methodName,
            Body = methodCode,
            Visibility = visibility,
            ReturnType = returnType,
            IsStatic = isStatic,
            IsAsync = isAsync,
            IsVirtual = isVirtual,
            IsOverride = isOverride,
            IsAbstract = isAbstract
        };
    }

    private static CodePropertyDefinition ParsePropertyCode(string propertyName, string propertyCode)
    {
        // Simple parsing - extract basic information from property signature
        var lines = propertyCode.Split('\n');
        var signatureLine = lines.FirstOrDefault(l => l.Trim().Contains(propertyName))?.Trim() ?? propertyCode.Trim();
        
        var visibility = "public";
        if (signatureLine.Contains("private ")) visibility = "private";
        else if (signatureLine.Contains("protected ")) visibility = "protected";
        else if (signatureLine.Contains("internal ")) visibility = "internal";
        
        var isStatic = signatureLine.Contains("static ");
        
        var propertyType = "string";
        // Try to extract property type - look for pattern before property name
        var parts = signatureLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i + 1].Contains(propertyName))
            {
                propertyType = parts[i];
                break;
            }
        }
        
        var hasGetter = propertyCode.Contains("get") || propertyCode.Contains("{ get;") || propertyCode.Contains("{get;");
        var hasSetter = propertyCode.Contains("set") || propertyCode.Contains("{ set;") || propertyCode.Contains("{set;");
        
        return new CodePropertyDefinition
        {
            Name = propertyName,
            Type = propertyType,
            Visibility = visibility,
            IsStatic = isStatic,
            HasGetter = hasGetter,
            HasSetter = hasSetter
        };
    }

    private static CodeFieldDefinition ParseFieldCode(string fieldName, string fieldCode)
    {
        // Simple parsing - extract basic information from field signature
        var lines = fieldCode.Split('\n');
        var signatureLine = lines.FirstOrDefault(l => l.Trim().Contains(fieldName))?.Trim() ?? fieldCode.Trim();
        
        var visibility = "private";
        if (signatureLine.Contains("public ")) visibility = "public";
        else if (signatureLine.Contains("protected ")) visibility = "protected";
        else if (signatureLine.Contains("internal ")) visibility = "internal";
        
        var isStatic = signatureLine.Contains("static ");
        var isReadonly = signatureLine.Contains("readonly ");
        
        var fieldType = "string";
        // Try to extract field type - look for pattern before field name
        var parts = signatureLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i + 1].Contains(fieldName))
            {
                fieldType = parts[i];
                break;
            }
        }
        
        var defaultValue = "";
        if (signatureLine.Contains("="))
        {
            var equalIndex = signatureLine.IndexOf('=');
            defaultValue = signatureLine.Substring(equalIndex + 1).Split(';')[0].Trim();
        }
        
        return new CodeFieldDefinition
        {
            Name = fieldName,
            Type = fieldType,
            Visibility = visibility,
            IsStatic = isStatic,
            IsReadonly = isReadonly,
            DefaultValue = defaultValue
        };
    }

    private static CodeTypeDefinition ParseTypeCode(string typeName, string typeKind, string typeCode)
    {
        var kind = typeKind.ToLower() switch
        {
            "class" => CodeTypeKind.Class,
            "interface" => CodeTypeKind.Interface,
            "struct" => CodeTypeKind.Struct,
            "enum" => CodeTypeKind.Enum,
            _ => throw new ArgumentException($"Unknown type kind: {typeKind}")
        };

        // Parse visibility from type code
        var visibility = "public";
        if (typeCode.Contains("internal ")) visibility = "internal";
        else if (typeCode.Contains("private ")) visibility = "private";
        else if (typeCode.Contains("protected ")) visibility = "protected";

        return new CodeTypeDefinition
        {
            Name = typeName,
            Kind = kind,
            Visibility = visibility,
            FilePath = "" // This will be set by the CreateType method when it gets the path parameter
        };
    }

    #endregion
}
