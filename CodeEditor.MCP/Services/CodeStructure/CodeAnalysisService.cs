using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeEditor.MCP.Models;
using System.IO.Abstractions;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Service responsible for analyzing and parsing C# code structures
/// </summary>
public class CodeAnalysisService : ICodeAnalysisService
{
    private readonly IFileSystem _fileSystem;
    private readonly IPathService _pathService;
    private readonly ICodeStructureCache _cache;

    public CodeAnalysisService(IFileSystem fileSystem, IPathService pathService, ICodeStructureCache cache)
    {
        _fileSystem = fileSystem;
        _pathService = pathService;
        _cache = cache;
    }

    public CodeTypeDefinition ParseType(string filePath, string typeName)
    {
        if (_cache.TryGetType(filePath, typeName, out var cachedType) && cachedType != null)
            return cachedType;

        var fullPath = _pathService.GetFullPath(filePath);
        var source = _fileSystem.File.ReadAllText(fullPath);
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var root = syntaxTree.GetRoot();

        var typeDeclaration = CodeSyntaxHelpers.FindTypeDeclaration(root, typeName);
        if (typeDeclaration == null)
            throw new InvalidOperationException($"Type '{typeName}' not found in file '{filePath}'");

        var typeDefinition = ParseTypeDeclaration(typeDeclaration, filePath);
        _cache.CacheType(filePath, typeDefinition);
        
        return typeDefinition;
    }

    public List<CodeTypeDefinition> ParseAllTypes(string filePath)
    {
        var fullPath = _pathService.GetFullPath(filePath);
        var source = _fileSystem.File.ReadAllText(fullPath);
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var root = syntaxTree.GetRoot();

        var types = new List<CodeTypeDefinition>();
        
        foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            var typeDefinition = ParseTypeDeclaration(typeDeclaration, filePath);
            types.Add(typeDefinition);
        }

        return types;
    }

    public ProjectStructure AnalyzeProject(string projectPath = ".")
    {
        var fullProjectPath = _pathService.GetFullPath(projectPath);
        
        var solutionFile = _fileSystem.Directory.GetFiles(fullProjectPath, "*.sln").FirstOrDefault();
        var projectFile = _fileSystem.Directory.GetFiles(fullProjectPath, "*.csproj").FirstOrDefault();
        
        if (solutionFile == null && projectFile == null)
            throw new InvalidOperationException("No solution or project file found");

        var projectStructure = new ProjectStructure
        {
            Name = Path.GetFileNameWithoutExtension(solutionFile ?? projectFile!),
            Path = fullProjectPath
        };

        return projectStructure;
    }

    private CodeTypeDefinition ParseTypeDeclaration(SyntaxNode typeDeclaration, string filePath)
    {
        return typeDeclaration switch
        {
            ClassDeclarationSyntax cls => ParseClassDeclaration(cls, filePath),
            InterfaceDeclarationSyntax iface => ParseInterfaceDeclaration(iface, filePath),
            StructDeclarationSyntax str => ParseStructDeclaration(str, filePath),
            EnumDeclarationSyntax enm => ParseEnumDeclaration(enm, filePath),
            _ => throw new NotSupportedException($"Type declaration {typeDeclaration.GetType()} is not supported")
        };
    }

    private CodeTypeDefinition ParseClassDeclaration(ClassDeclarationSyntax classDeclaration, string filePath)
    {
        var lineSpan = classDeclaration.GetLocation().GetLineSpan();
        
        var definition = new CodeTypeDefinition
        {
            Name = classDeclaration.Identifier.ValueText,
            Kind = CodeTypeKind.Class,
            FilePath = filePath,
            Visibility = CodeSyntaxHelpers.GetVisibility(classDeclaration.Modifiers),
            IsStatic = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
            IsAbstract = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
            IsSealed = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)),
            IsPartial = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
            StartLine = lineSpan.StartLinePosition.Line + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1
        };

        if (classDeclaration.BaseList != null)
        {
            var baseTypes = classDeclaration.BaseList.Types.Select(t => t.Type.ToString()).ToList();
            if (baseTypes.Any())
            {
                definition.BaseType = baseTypes.First();
                definition.Interfaces = baseTypes.Skip(1).ToList();
            }
        }

        ParseMembersFromType(classDeclaration, definition);
        return definition;
    }

    private CodeTypeDefinition ParseInterfaceDeclaration(InterfaceDeclarationSyntax interfaceDeclaration, string filePath)
    {
        var lineSpan = interfaceDeclaration.GetLocation().GetLineSpan();
        
        var definition = new CodeTypeDefinition
        {
            Name = interfaceDeclaration.Identifier.ValueText,
            Kind = CodeTypeKind.Interface,
            FilePath = filePath,
            Visibility = CodeSyntaxHelpers.GetVisibility(interfaceDeclaration.Modifiers),
            StartLine = lineSpan.StartLinePosition.Line + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1
        };

        if (interfaceDeclaration.BaseList != null)
        {
            definition.Interfaces = interfaceDeclaration.BaseList.Types
                .Select(t => t.Type.ToString()).ToList();
        }

        ParseMembersFromType(interfaceDeclaration, definition);
        return definition;
    }

    private CodeTypeDefinition ParseStructDeclaration(StructDeclarationSyntax structDeclaration, string filePath)
    {
        var lineSpan = structDeclaration.GetLocation().GetLineSpan();
        
        var definition = new CodeTypeDefinition
        {
            Name = structDeclaration.Identifier.ValueText,
            Kind = CodeTypeKind.Struct,
            FilePath = filePath,
            Visibility = CodeSyntaxHelpers.GetVisibility(structDeclaration.Modifiers),
            IsPartial = structDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
            StartLine = lineSpan.StartLinePosition.Line + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1
        };

        if (structDeclaration.BaseList != null)
        {
            definition.Interfaces = structDeclaration.BaseList.Types
                .Select(t => t.Type.ToString()).ToList();
        }

        ParseMembersFromType(structDeclaration, definition);
        return definition;
    }

    private CodeTypeDefinition ParseEnumDeclaration(EnumDeclarationSyntax enumDeclaration, string filePath)
    {
        var lineSpan = enumDeclaration.GetLocation().GetLineSpan();
        
        return new CodeTypeDefinition
        {
            Name = enumDeclaration.Identifier.ValueText,
            Kind = CodeTypeKind.Enum,
            FilePath = filePath,
            Visibility = CodeSyntaxHelpers.GetVisibility(enumDeclaration.Modifiers),
            StartLine = lineSpan.StartLinePosition.Line + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1
        };
    }

    private void ParseMembersFromType(SyntaxNode typeDeclaration, CodeTypeDefinition definition)
    {
        foreach (var member in typeDeclaration.ChildNodes())
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    definition.Members.Methods.Add(ParseMethodDeclaration(method));
                    break;
                case PropertyDeclarationSyntax property:
                    definition.Members.Properties.Add(ParsePropertyDeclaration(property));
                    break;
                case FieldDeclarationSyntax field:
                    definition.Members.Fields.AddRange(ParseFieldDeclaration(field));
                    break;
                case EventDeclarationSyntax eventDecl:
                    definition.Members.Events.Add(ParseEventDeclaration(eventDecl));
                    break;
            }
        }
    }

    private CodeMethodDefinition ParseMethodDeclaration(MethodDeclarationSyntax method)
    {
        var lineSpan = method.GetLocation().GetLineSpan();
        
        return new CodeMethodDefinition
        {
            Name = method.Identifier.ValueText,
            ReturnType = method.ReturnType.ToString(),
            Visibility = CodeSyntaxHelpers.GetVisibility(method.Modifiers),
            IsStatic = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
            IsVirtual = method.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
            IsOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
            IsAbstract = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
            IsAsync = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)),
            Body = method.Body?.ToString(),
            Parameters = method.ParameterList.Parameters.Select(CodeSyntaxHelpers.ParseParameter).ToList(),
            StartLine = lineSpan.StartLinePosition.Line + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1
        };
    }

    private CodePropertyDefinition ParsePropertyDeclaration(PropertyDeclarationSyntax property)
    {
        var lineSpan = property.GetLocation().GetLineSpan();
        
        return new CodePropertyDefinition
        {
            Name = property.Identifier.ValueText,
            Type = property.Type.ToString(),
            Visibility = CodeSyntaxHelpers.GetVisibility(property.Modifiers),
            IsStatic = property.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
            IsVirtual = property.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
            IsOverride = property.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
            HasGetter = property.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false,
            HasSetter = property.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false,
            StartLine = lineSpan.StartLinePosition.Line + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1
        };
    }

    private List<CodeFieldDefinition> ParseFieldDeclaration(FieldDeclarationSyntax field)
    {
        var lineSpan = field.GetLocation().GetLineSpan();
        var fields = new List<CodeFieldDefinition>();
        
        foreach (var variable in field.Declaration.Variables)
        {
            fields.Add(new CodeFieldDefinition
            {
                Name = variable.Identifier.ValueText,
                Type = field.Declaration.Type.ToString(),
                Visibility = CodeSyntaxHelpers.GetVisibility(field.Modifiers),
                IsStatic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                IsReadonly = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)),
                IsConst = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)),
                DefaultValue = variable.Initializer?.Value.ToString(),
                StartLine = lineSpan.StartLinePosition.Line + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1
            });
        }
        
        return fields;
    }

    private CodeEventDefinition ParseEventDeclaration(EventDeclarationSyntax eventDecl)
    {
        var lineSpan = eventDecl.GetLocation().GetLineSpan();
        
        return new CodeEventDefinition
        {
            Name = eventDecl.Identifier.ValueText,
            Type = eventDecl.Type.ToString(),
            Visibility = CodeSyntaxHelpers.GetVisibility(eventDecl.Modifiers),
            IsStatic = eventDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
            StartLine = lineSpan.StartLinePosition.Line + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1
        };
    }
}
