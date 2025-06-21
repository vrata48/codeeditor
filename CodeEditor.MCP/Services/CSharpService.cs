using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO.Abstractions;
using Microsoft.CodeAnalysis;

namespace CodeEditor.MCP.Services;

public class CSharpService(IFileSystem fileSystem, IPathService pathService) : ICSharpService
{
    public string[] AnalyzeFile(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();
        
        var results = new List<string>();
        
        foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            results.Add($"Class: {classDecl.Identifier.ValueText}");
            
            foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
            {
                var parameters = string.Join(", ", method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
                results.Add($"  Method: {method.ReturnType} {method.Identifier}({parameters})");
            }
        }
        
        return results.ToArray();
    }
    
    public void AddMethod(string relativePath, string className, string methodCode)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();
        
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == className);
        
        if (classDecl != null)
        {
            var method = CSharpSyntaxTree.ParseText(methodCode).GetRoot()
                .DescendantNodes().OfType<MethodDeclarationSyntax>().First();
            
            var newClass = classDecl.AddMembers(method);
            var newRoot = root.ReplaceNode(classDecl, newClass);
            
            fileSystem.File.WriteAllText(fullPath, newRoot.ToFullString());
        }
    }
    
    public void ReplaceMethod(string relativePath, string className, string oldMethodName, string newMethodCode)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();
        
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == className);
        
        if (classDecl != null)
        {
            var oldMethod = classDecl.Members.OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == oldMethodName);
            
            if (oldMethod != null)
            {
                var newMethod = CSharpSyntaxTree.ParseText(newMethodCode).GetRoot()
                    .DescendantNodes().OfType<MethodDeclarationSyntax>().First();
                
                var newClass = classDecl.ReplaceNode(oldMethod, newMethod);
                var newRoot = root.ReplaceNode(classDecl, newClass);
                
                fileSystem.File.WriteAllText(fullPath, newRoot.ToFullString());
            }
        }
    }
    
    public void RemoveMethod(string relativePath, string className, string methodName)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();
        
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == className);
        
        if (classDecl != null)
        {
            var method = classDecl.Members.OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == methodName);
            
            if (method != null)
            {
                var newClass = classDecl.RemoveNode(method, SyntaxRemoveOptions.KeepNoTrivia);
                var newRoot = root.ReplaceNode(classDecl, newClass!);
                
                fileSystem.File.WriteAllText(fullPath, newRoot.ToFullString());
            }
        }
    }
}
