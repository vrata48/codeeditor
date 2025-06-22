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

            foreach (var property in classDecl.Members.OfType<PropertyDeclarationSyntax>())
            {
                results.Add($"  Property: {property.Type} {property.Identifier}");
            }

            foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
            {
                var parameters = string.Join(", ",
                    method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
                results.Add($"  Method: {method.ReturnType} {method.Identifier}({parameters})");
            }
        }

        foreach (var interfaceDecl in root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
        {
            results.Add($"Interface: {interfaceDecl.Identifier.ValueText}");

            foreach (var property in interfaceDecl.Members.OfType<PropertyDeclarationSyntax>())
            {
                results.Add($"  Property: {property.Type} {property.Identifier}");
            }

            foreach (var method in interfaceDecl.Members.OfType<MethodDeclarationSyntax>())
            {
                var parameters = string.Join(", ",
                    method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
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
            // Try to parse the method code and extract the first method
            var wrappedCode = $"class DummyClass {{ {methodCode} }}";
            var methodTree = CSharpSyntaxTree.ParseText(wrappedCode);
            var methodRoot = methodTree.GetRoot();
            var method = methodRoot.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            if (method == null)
            {
                throw new ArgumentException(
                    $"No valid method declaration found in the provided method code: {methodCode}");
            }

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
                // Try to parse the new method code and extract the first method
                var wrappedCode = $"class DummyClass {{ {newMethodCode} }}";
                var methodTree = CSharpSyntaxTree.ParseText(wrappedCode);
                var methodRoot = methodTree.GetRoot();
                var newMethod = methodRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();

                if (newMethod == null)
                {
                    throw new ArgumentException(
                        $"No valid method declaration found in the provided method code: {newMethodCode}");
                }

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

    public void AddProperty(string relativePath, string className, string propertyCode)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();

        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == className);

        if (classDecl != null)
        {
            // Try to parse the property code and extract the first property
            var wrappedCode = $"class DummyClass {{ {propertyCode} }}";
            var propertyTree = CSharpSyntaxTree.ParseText(wrappedCode);
            var propertyRoot = propertyTree.GetRoot();
            var property = propertyRoot.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault();

            if (property == null)
            {
                throw new ArgumentException(
                    $"No valid property declaration found in the provided property code: {propertyCode}");
            }

            var newClass = classDecl.AddMembers(property);
            var newRoot = root.ReplaceNode(classDecl, newClass);

            fileSystem.File.WriteAllText(fullPath, newRoot.ToFullString());
        }
    }

    public void ReplaceProperty(string relativePath, string className, string oldPropertyName, string newPropertyCode)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();

        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == className);

        if (classDecl != null)
        {
            var oldProperty = classDecl.Members.OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.ValueText == oldPropertyName);

            if (oldProperty != null)
            {
                // Try to parse the new property code and extract the first property
                var wrappedCode = $"class DummyClass {{ {newPropertyCode} }}";
                var propertyTree = CSharpSyntaxTree.ParseText(wrappedCode);
                var propertyRoot = propertyTree.GetRoot();
                var newProperty = propertyRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>().FirstOrDefault();

                if (newProperty == null)
                {
                    throw new ArgumentException(
                        $"No valid property declaration found in the provided property code: {newPropertyCode}");
                }

                var newClass = classDecl.ReplaceNode(oldProperty, newProperty);
                var newRoot = root.ReplaceNode(classDecl, newClass);

                fileSystem.File.WriteAllText(fullPath, newRoot.ToFullString());
            }
        }
    }

    public void RemoveProperty(string relativePath, string className, string propertyName)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();

        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.ValueText == className);

        if (classDecl != null)
        {
            var property = classDecl.Members.OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.ValueText == propertyName);

            if (property != null)
            {
                var newClass = classDecl.RemoveNode(property, SyntaxRemoveOptions.KeepNoTrivia);
                var newRoot = root.ReplaceNode(classDecl, newClass!);

                fileSystem.File.WriteAllText(fullPath, newRoot.ToFullString());
            }
        }
    }
public void CreateInterface(string relativePath, string interfaceName, string interfaceCode)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        
        // If file doesn't exist, create it with basic structure
        if (!fileSystem.File.Exists(fullPath))
        {
            var namespaceFromPath = pathService.GetNamespaceFromPath(relativePath);
            var basicContent = $"namespace {namespaceFromPath};\n\n{interfaceCode}";
            fileSystem.File.WriteAllText(fullPath, basicContent);
            return;
        }

        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();

        // Parse the interface code to extract the interface declaration
        var interfaceTree = CSharpSyntaxTree.ParseText(interfaceCode);
        var interfaceRoot = interfaceTree.GetRoot();
        var interfaceDecl = interfaceRoot.DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault();

        if (interfaceDecl == null)
        {
            throw new ArgumentException($"No valid interface declaration found in the provided interface code: {interfaceCode}");
        }

        // Check if root is a CompilationUnitSyntax to use AddMembers
        if (root is CompilationUnitSyntax compilationUnit)
        {
            var newCompilationUnit = compilationUnit.AddMembers(interfaceDecl);
            fileSystem.File.WriteAllText(fullPath, newCompilationUnit.ToFullString());
        }
        else
        {
            throw new InvalidOperationException("Unable to add interface to the file structure");
        }
    } public void AddMethodToInterface(string relativePath, string interfaceName, string methodSignature)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();

        var interfaceDecl = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => i.Identifier.ValueText == interfaceName);

        if (interfaceDecl != null)
        {
            // Parse the method signature and extract the method
            var wrappedCode = $"interface DummyInterface {{ {methodSignature} }}";
            var methodTree = CSharpSyntaxTree.ParseText(wrappedCode);
            var methodRoot = methodTree.GetRoot();
            var method = methodRoot.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            if (method == null)
            {
                throw new ArgumentException($"No valid method declaration found in the provided method signature: {methodSignature}");
            }

            var newInterface = interfaceDecl.AddMembers(method);
            var newRoot = root.ReplaceNode(interfaceDecl, newInterface);

            fileSystem.File.WriteAllText(fullPath, newRoot.ToFullString());
        }
    } public void ReplaceMethodInInterface(string relativePath, string interfaceName, string oldMethodName, string newMethodSignature)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();

        var interfaceDecl = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => i.Identifier.ValueText == interfaceName);

        if (interfaceDecl != null)
        {
            var oldMethod = interfaceDecl.Members.OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == oldMethodName);

            if (oldMethod != null)
            {
                // Parse the new method signature and extract the method
                var wrappedCode = $"interface DummyInterface {{ {newMethodSignature} }}";
                var methodTree = CSharpSyntaxTree.ParseText(wrappedCode);
                var methodRoot = methodTree.GetRoot();
                var newMethod = methodRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();

                if (newMethod == null)
                {
                    throw new ArgumentException($"No valid method declaration found in the provided method signature: {newMethodSignature}");
                }

                var newInterface = interfaceDecl.ReplaceNode(oldMethod, newMethod);
                var newRoot = root.ReplaceNode(interfaceDecl, newInterface);

                fileSystem.File.WriteAllText(fullPath, newRoot.ToFullString());
            }
        }
    } public void RemoveMethodFromInterface(string relativePath, string interfaceName, string methodName)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetRoot();

        var interfaceDecl = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => i.Identifier.ValueText == interfaceName);

        if (interfaceDecl != null)
        {
            var method = interfaceDecl.Members.OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == methodName);

            if (method != null)
            {
                var newInterface = interfaceDecl.RemoveNode(method, SyntaxRemoveOptions.KeepNoTrivia);
                var newRoot = root.ReplaceNode(interfaceDecl, newInterface!);

                fileSystem.File.WriteAllText(fullPath, newRoot.ToFullString());
            }
        }
    } }