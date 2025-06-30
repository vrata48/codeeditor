using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Helper class for common syntax operations used across code structure services
/// </summary>
public static class CodeSyntaxHelpers
{
    public static SyntaxNode? FindTypeDeclaration(SyntaxNode root, string typeName)
    {
        return root.DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault(t => t.Identifier.ValueText == typeName);
    }

    public static string GetVisibility(SyntaxTokenList modifiers)
    {
        if (modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) return "public";
        if (modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword))) return "private";
        if (modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword))) return "protected";
        if (modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword))) return "internal";
        return "private";
    }

    public static MethodDeclarationSyntax? FindMethodInType(SyntaxNode typeDeclaration, string methodName)
    {
        return typeDeclaration.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == methodName);
    }

    public static PropertyDeclarationSyntax? FindPropertyInType(SyntaxNode typeDeclaration, string propertyName)
    {
        return typeDeclaration.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(p => p.Identifier.ValueText == propertyName);
    }

    public static FieldDeclarationSyntax? FindFieldInType(SyntaxNode typeDeclaration, string fieldName)
    {
        return typeDeclaration.DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .FirstOrDefault(f => f.Declaration.Variables.Any(v => v.Identifier.ValueText == fieldName));
    }

    public static TypeDeclarationSyntax AddMemberToType(SyntaxNode typeDeclaration, MemberDeclarationSyntax member)
    {
        return typeDeclaration switch
        {
            ClassDeclarationSyntax cls => cls.AddMembers(member),
            InterfaceDeclarationSyntax iface => iface.AddMembers(member),
            StructDeclarationSyntax str => str.AddMembers(member),
            _ => throw new InvalidOperationException($"Unsupported type declaration: {typeDeclaration.GetType()}")
        };
    }

    public static TypeDeclarationSyntax RemoveMemberFromType(SyntaxNode typeDeclaration, SyntaxNode member)
    {
        return typeDeclaration switch
        {
            ClassDeclarationSyntax cls => cls.RemoveNode(member, SyntaxRemoveOptions.KeepNoTrivia)!,
            InterfaceDeclarationSyntax iface => iface.RemoveNode(member, SyntaxRemoveOptions.KeepNoTrivia)!,
            StructDeclarationSyntax str => str.RemoveNode(member, SyntaxRemoveOptions.KeepNoTrivia)!,
            _ => throw new InvalidOperationException($"Unsupported type declaration: {typeDeclaration.GetType()}")
        };
    }

    public static CodeParameterDefinition ParseParameter(ParameterSyntax parameter)
    {
        return new CodeParameterDefinition
        {
            Name = parameter.Identifier.ValueText,
            Type = parameter.Type?.ToString() ?? "",
            DefaultValue = parameter.Default?.Value.ToString(),
            IsOut = parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword)),
            IsRef = parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword)),
            IsParams = parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.ParamsKeyword))
        };
    }
}
