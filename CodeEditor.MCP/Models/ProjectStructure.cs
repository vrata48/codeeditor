namespace CodeEditor.MCP.Models;

public class ProjectStructure
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<CodeFile> Files { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
}

public class CodeFile
{
    public string Name { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public List<CodeClass> Classes { get; set; } = new();
    public List<CodeInterface> Interfaces { get; set; } = new();
    public List<CodeEnum> Enums { get; set; } = new();
    public List<CodeStruct> Structs { get; set; } = new();
    public List<CodeDelegate> Delegates { get; set; } = new();
    public List<string> Usings { get; set; } = new();
}

public class CodeClass
{
    public string Name { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }
    public bool IsPartial { get; set; }
    public string? BaseClass { get; set; }
    public List<string> Interfaces { get; set; } = new();
    public List<CodeProperty> Properties { get; set; } = new();
    public List<CodeMethod> Methods { get; set; } = new();
    public List<CodeField> Fields { get; set; } = new();
    public List<CodeConstructor> Constructors { get; set; } = new();
    public List<CodeEvent> Events { get; set; } = new();
    
    // Nested types
    public List<CodeClass> NestedClasses { get; set; } = new();
    public List<CodeInterface> NestedInterfaces { get; set; } = new();
    public List<CodeEnum> NestedEnums { get; set; } = new();
    public List<CodeStruct> NestedStructs { get; set; } = new();
    public List<CodeDelegate> NestedDelegates { get; set; } = new();
    
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class CodeInterface
{
    public string Name { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public List<string> BaseInterfaces { get; set; } = new();
    public List<CodeMethod> Methods { get; set; } = new();
    public List<CodeProperty> Properties { get; set; } = new();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class CodeEnum
{
    public string Name { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string? UnderlyingType { get; set; } // int, byte, etc.
    public List<CodeEnumMember> Members { get; set; } = new();
    public List<string> Attributes { get; set; } = new();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class CodeEnumMember
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public List<string> Attributes { get; set; } = new();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class CodeStruct
{
    public string Name { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsReadonly { get; set; }
    public bool IsRef { get; set; }
    public List<string> Interfaces { get; set; } = new();
    public List<CodeProperty> Properties { get; set; } = new();
    public List<CodeMethod> Methods { get; set; } = new();
    public List<CodeField> Fields { get; set; } = new();
    public List<CodeConstructor> Constructors { get; set; } = new();
    
    // Nested types
    public List<CodeClass> NestedClasses { get; set; } = new();
    public List<CodeInterface> NestedInterfaces { get; set; } = new();
    public List<CodeEnum> NestedEnums { get; set; } = new();
    public List<CodeStruct> NestedStructs { get; set; } = new();
    public List<CodeDelegate> NestedDelegates { get; set; } = new();
    
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class CodeDelegate
{
    public string Name { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<CodeParameter> Parameters { get; set; } = new();
    public List<string> Attributes { get; set; } = new();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class CodeMethod
{
    public string Name { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<CodeParameter> Parameters { get; set; } = new();
    public bool IsStatic { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsAsync { get; set; }
    public List<string> Attributes { get; set; } = new();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string? Body { get; set; } // Optional - can be loaded on demand
}

public class CodeProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public bool IsStatic { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public List<string> Attributes { get; set; } = new();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string? GetterBody { get; set; }
    public string? SetterBody { get; set; }
}

public class CodeField
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsConst { get; set; }
    public string? DefaultValue { get; set; }
    public List<string> Attributes { get; set; } = new();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class CodeConstructor
{
    public string Name { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public List<CodeParameter> Parameters { get; set; } = new();
    public List<string> Attributes { get; set; } = new();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string? Body { get; set; }
    public string? BaseConstructorCall { get; set; } // : base(param1, param2)
    public string? ThisConstructorCall { get; set; } // : this(param1, param2)
}

public class CodeEvent
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public List<string> Attributes { get; set; } = new();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class CodeParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public bool IsOut { get; set; }
    public bool IsRef { get; set; }
    public bool IsParams { get; set; }
}
