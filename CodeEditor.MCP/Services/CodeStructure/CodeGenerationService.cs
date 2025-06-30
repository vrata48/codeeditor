using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Service responsible for generating C# syntax from code definitions
/// </summary>
public class CodeGenerationService : ICodeGenerationService
{
    public string GenerateCode(CodeTypeDefinition type)
    {
        var syntaxNode = GenerateTypeSyntax(type);
        return syntaxNode.NormalizeWhitespace().ToFullString();
    }

    public TypeDeclarationSyntax GenerateTypeSyntax(CodeTypeDefinition type)
    {
        return type.Kind switch
        {
            CodeTypeKind.Class => GenerateClassSyntax(type),
            CodeTypeKind.Interface => GenerateInterfaceSyntax(type),
            CodeTypeKind.Struct => GenerateStructSyntax(type),
            _ => throw new NotSupportedException($"Type kind {type.Kind} is not supported")
        };
    }

    private ClassDeclarationSyntax GenerateClassSyntax(CodeTypeDefinition type)
    {
        var classDeclaration = SyntaxFactory.ClassDeclaration(type.Name)
            .AddModifiers(GetModifierTokens(type).ToArray());

        // Add base class and interfaces
        if (type.BaseType != null || type.Interfaces.Any())
        {
            var baseList = new List<BaseTypeSyntax>();
            
            if (type.BaseType != null)
            {
                baseList.Add(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(type.BaseType)));
            }
            
            foreach (var iface in type.Interfaces)
            {
                baseList.Add(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(iface)));
            }
            
            classDeclaration = classDeclaration.WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SeparatedList(baseList)));
        }

        return AddMembers(classDeclaration, type);
    }

    private InterfaceDeclarationSyntax GenerateInterfaceSyntax(CodeTypeDefinition type)
    {
        var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration(type.Name)
            .AddModifiers(GetModifierTokens(type).ToArray());

        // Add interfaces
        if (type.Interfaces.Any())
        {
            var baseList = type.Interfaces.Select(iface => 
                (BaseTypeSyntax)SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(iface))).ToList();
            
            interfaceDeclaration = interfaceDeclaration.WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SeparatedList(baseList)));
        }

        return AddMembers(interfaceDeclaration, type);
    }

    private StructDeclarationSyntax GenerateStructSyntax(CodeTypeDefinition type)
    {
        var structDeclaration = SyntaxFactory.StructDeclaration(type.Name)
            .AddModifiers(GetModifierTokens(type).ToArray());

        // Add interfaces
        if (type.Interfaces.Any())
        {
            var baseList = type.Interfaces.Select(iface => 
                (BaseTypeSyntax)SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(iface))).ToList();
            
            structDeclaration = structDeclaration.WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SeparatedList(baseList)));
        }

        return AddMembers(structDeclaration, type);
    }

    private T AddMembers<T>(T typeDeclaration, CodeTypeDefinition type) 
        where T : TypeDeclarationSyntax
    {
        var members = new List<MemberDeclarationSyntax>();

        // Add fields
        foreach (var field in type.Members.Fields)
        {
            members.Add(GenerateFieldSyntax(field));
        }

        // Add properties
        foreach (var property in type.Members.Properties)
        {
            members.Add(GeneratePropertySyntax(property));
        }

        // Add methods
        foreach (var method in type.Members.Methods)
        {
            members.Add(GenerateMethodSyntax(method));
        }

        // Add events
        foreach (var eventDef in type.Members.Events)
        {
            members.Add(GenerateEventSyntax(eventDef));
        }

        return (T)typeDeclaration.WithMembers(SyntaxFactory.List(members));
    }

    private FieldDeclarationSyntax GenerateFieldSyntax(CodeFieldDefinition field)
    {
        var variableDeclarator = SyntaxFactory.VariableDeclarator(field.Name);
        
        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            variableDeclarator = variableDeclarator.WithInitializer(
                SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(field.DefaultValue)));
        }

        var variableDeclaration = SyntaxFactory.VariableDeclaration(
            SyntaxFactory.ParseTypeName(field.Type))
            .AddVariables(variableDeclarator);

        return SyntaxFactory.FieldDeclaration(variableDeclaration)
            .AddModifiers(GetFieldModifierTokens(field).ToArray());
    }

    private PropertyDeclarationSyntax GeneratePropertySyntax(CodePropertyDefinition property)
    {
        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.ParseTypeName(property.Type), property.Name)
            .AddModifiers(GetPropertyModifierTokens(property).ToArray());

        var accessors = new List<AccessorDeclarationSyntax>();

        if (property.HasGetter)
        {
            accessors.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }

        if (property.HasSetter)
        {
            accessors.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }

        if (accessors.Any())
        {
            propertyDeclaration = propertyDeclaration.WithAccessorList(
                SyntaxFactory.AccessorList(SyntaxFactory.List(accessors)));
        }

        return propertyDeclaration;
    }

    private MethodDeclarationSyntax GenerateMethodSyntax(CodeMethodDefinition method)
    {
        var methodDeclaration = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.ParseTypeName(method.ReturnType), method.Name)
            .AddModifiers(GetMethodModifierTokens(method).ToArray());

        // Add parameters
        var parameters = method.Parameters.Select(p => SyntaxFactory.Parameter(
            SyntaxFactory.Identifier(p.Name))
            .WithType(SyntaxFactory.ParseTypeName(p.Type)));

        methodDeclaration = methodDeclaration.WithParameterList(
            SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)));

        // Add body if present
        if (!string.IsNullOrEmpty(method.Body))
        {
            var body = SyntaxFactory.ParseStatement(method.Body);
            if (body is BlockSyntax block)
            {
                methodDeclaration = methodDeclaration.WithBody(block);
            }
            else
            {
                methodDeclaration = methodDeclaration.WithBody(
                    SyntaxFactory.Block(body));
            }
        }
        else if (method.IsAbstract)
        {
            methodDeclaration = methodDeclaration.WithSemicolonToken(
                SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
        else
        {
            methodDeclaration = methodDeclaration.WithBody(SyntaxFactory.Block());
        }

        return methodDeclaration;
    }

    private EventDeclarationSyntax GenerateEventSyntax(CodeEventDefinition eventDef)
    {
        return SyntaxFactory.EventDeclaration(
            SyntaxFactory.ParseTypeName(eventDef.Type), eventDef.Name)
            .AddModifiers(GetEventModifierTokens(eventDef).ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private IEnumerable<SyntaxToken> GetModifierTokens(CodeTypeDefinition type)
    {
        var modifiers = new List<SyntaxToken>();

        // Visibility
        switch (type.Visibility?.ToLower())
        {
            case "public":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                break;
            case "internal":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                break;
            case "private":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                break;
            case "protected":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                break;
        }

        // Class-specific modifiers
        if (type.IsStatic)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        if (type.IsAbstract)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.AbstractKeyword));
        if (type.IsSealed)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword));
        if (type.IsPartial)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        return modifiers;
    }

    private IEnumerable<SyntaxToken> GetFieldModifierTokens(CodeFieldDefinition field)
    {
        var modifiers = new List<SyntaxToken>();

        // Visibility
        switch (field.Visibility?.ToLower())
        {
            case "public":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                break;
            case "internal":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                break;
            case "private":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                break;
            case "protected":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                break;
        }

        if (field.IsStatic)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        if (field.IsReadonly)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
        if (field.IsConst)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.ConstKeyword));

        return modifiers;
    }

    private IEnumerable<SyntaxToken> GetPropertyModifierTokens(CodePropertyDefinition property)
    {
        var modifiers = new List<SyntaxToken>();

        // Visibility
        switch (property.Visibility?.ToLower())
        {
            case "public":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                break;
            case "internal":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                break;
            case "private":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                break;
            case "protected":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                break;
        }

        if (property.IsStatic)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        if (property.IsVirtual)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
        if (property.IsOverride)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword));

        return modifiers;
    }

    private IEnumerable<SyntaxToken> GetMethodModifierTokens(CodeMethodDefinition method)
    {
        var modifiers = new List<SyntaxToken>();

        // Visibility
        switch (method.Visibility?.ToLower())
        {
            case "public":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                break;
            case "internal":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                break;
            case "private":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                break;
            case "protected":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                break;
        }

        if (method.IsStatic)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        if (method.IsVirtual)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
        if (method.IsOverride)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
        if (method.IsAbstract)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.AbstractKeyword));
        if (method.IsAsync)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));

        return modifiers;
    }

    private IEnumerable<SyntaxToken> GetEventModifierTokens(CodeEventDefinition eventDef)
    {
        var modifiers = new List<SyntaxToken>();

        // Visibility
        switch (eventDef.Visibility?.ToLower())
        {
            case "public":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                break;
            case "internal":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                break;
            case "private":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                break;
            case "protected":
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                break;
        }

        if (eventDef.IsStatic)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

        return modifiers;
    }
}
