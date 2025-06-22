using System.ComponentModel;
using CodeEditor.MCP.Services;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
public static class CSharpTools
{
[McpServerTool]
    [Description("Analyze C# file for classes and methods")]
    public static string AnalyzeFile(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path)
    {
        return service.AnalyzeFile(path);
    }     
    [McpServerTool]
    [Description("Add method to C# class")]
    public static void AddMethod(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class")] string className,
        [Description("Complete method code")] string methodCode)
    {
        service.AddMethod(path, className, methodCode);
    }
    
    [McpServerTool]
    [Description("Replace existing method in C# class")]
    public static void ReplaceMethod(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class")] string className,
        [Description("Name of method to replace")] string oldMethodName,
        [Description("Complete new method code")] string newMethodCode)
    {
        service.ReplaceMethod(path, className, oldMethodName, newMethodCode);
    }
    
    [McpServerTool]
    [Description("Remove method from C# class")]
    public static void RemoveMethod(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class")] string className,
        [Description("Name of method to remove")] string methodName)
    {
        service.RemoveMethod(path, className, methodName);
    }
    
    [McpServerTool]
    [Description("Add property to C# class")]
    public static void AddProperty(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class")] string className,
        [Description("Complete property code")] string propertyCode)
    {
        service.AddProperty(path, className, propertyCode);
    }
    
    [McpServerTool]
    [Description("Replace existing property in C# class")]
    public static void ReplaceProperty(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class")] string className,
        [Description("Name of property to replace")] string oldPropertyName,
        [Description("Complete new property code")] string newPropertyCode)
    {
        service.ReplaceProperty(path, className, oldPropertyName, newPropertyCode);
    }
    
    [McpServerTool]
    [Description("Remove property from C# class")]
    public static void RemoveProperty(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the class")] string className,
        [Description("Name of property to remove")] string propertyName)
    {
        service.RemoveProperty(path, className, propertyName);
    }
[McpServerTool]
    [Description("Create new C# interface")]
    public static void CreateInterface(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the interface")] string interfaceName,
        [Description("Complete interface code")] string interfaceCode)
    {
        service.CreateInterface(path, interfaceName, interfaceCode);
    } [McpServerTool]
    [Description("Add method to C# interface")]
    public static void AddMethodToInterface(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the interface")] string interfaceName,
        [Description("Method signature (without body)")] string methodSignature)
    {
        service.AddMethodToInterface(path, interfaceName, methodSignature);
    } [McpServerTool]
    [Description("Replace existing method in C# interface")]
    public static void ReplaceMethodInInterface(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the interface")] string interfaceName,
        [Description("Name of method to replace")] string oldMethodName,
        [Description("New method signature (without body)")] string newMethodSignature)
    {
        service.ReplaceMethodInInterface(path, interfaceName, oldMethodName, newMethodSignature);
    } [McpServerTool]
    [Description("Remove method from C# interface")]
    public static void RemoveMethodFromInterface(
        ICSharpService service,
        [Description("Relative path to .cs file")] string path,
        [Description("Name of the interface")] string interfaceName,
        [Description("Name of method to remove")] string methodName)
    {
        service.RemoveMethodFromInterface(path, interfaceName, methodName);
    } }
