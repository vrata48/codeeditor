namespace CodeEditor.MCP.Models;

/// <summary>
/// Simplified representation of a C# type for LLM consumption
/// Contains only the essential structural information needed for understanding
/// </summary>
public class CodeTypeDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public CodeTypeKind Kind { get; set; }
    public string Visibility { get; set; } = string.Empty;
    
    // Essential modifiers only
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }
    public bool IsPartial { get; set; }
    
    // Inheritance - simplified
    public string? BaseType { get; set; }
    public List<string> Interfaces { get; set; } = new();
    
    // Simplified member collection
    public CodeMemberCollection Members { get; set; } = new();
    
    // Essential metadata only
    public List<string> Usings { get; set; } = new();
    public List<string> Attributes { get; set; } = new();
    public string? Documentation { get; set; }
    
    // Position information
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    
    // Method helpers
    public CodeMethodDefinition? FindMethod(string methodName)
    {
        return Members.Methods.FirstOrDefault(m => m.Name == methodName);
    }
    
    public void ReplaceMethod(CodeMethodDefinition oldMethod, CodeMethodDefinition newMethod)
    {
        var index = Members.Methods.IndexOf(oldMethod);
        if (index >= 0)
        {
            Members.Methods[index] = newMethod;
        }
    }
}

public class CodeMemberCollection
{
    public List<CodeMethodDefinition> Methods { get; set; } = new();
    public List<CodePropertyDefinition> Properties { get; set; } = new();
    public List<CodeFieldDefinition> Fields { get; set; } = new();
    public List<CodeEventDefinition> Events { get; set; } = new();
}

/// <summary>
/// Simplified method definition - no body, minimal details
/// </summary>
public class CodeMethodDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<CodeParameterDefinition> Parameters { get; set; } = new();
    public bool IsStatic { get; set; }
    public bool IsAsync { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public bool IsAbstract { get; set; }
    
    // Keep body for modification operations, but make it optional for LLM viewing
    public string? Body { get; set; }
    
    // Additional metadata
    public List<string> Attributes { get; set; } = new();
    public string? Documentation { get; set; }
    
    // Position information
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

/// <summary>
/// Simplified property definition
/// </summary>
public class CodePropertyDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public bool IsStatic { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    
    // Position information
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    
    // Property bodies for modification operations
    public string? GetterBody { get; set; }
    public string? SetterBody { get; set; }
}

/// <summary>
/// Simplified field definition
/// </summary>
public class CodeFieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsConst { get; set; }
    public string? DefaultValue { get; set; }
    
    // Position information
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

/// <summary>
/// Simplified event definition
/// </summary>
public class CodeEventDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    
    // Position information
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

/// <summary>
/// Simplified parameter definition - just the essentials
/// </summary>
public class CodeParameterDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public bool IsOut { get; set; }
    public bool IsRef { get; set; }
    public bool IsParams { get; set; }
}

public enum CodeTypeKind
{
    Class,
    Interface,
    Struct,
    Enum,
    Delegate
}
