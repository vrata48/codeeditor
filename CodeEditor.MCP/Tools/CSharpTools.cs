using System.ComponentModel;
using CodeEditor.MCP.Services;
using ModelContextProtocol.Server;

namespace CodeEditor.MCP.Tools;

[McpServerToolType]
public static class CSharpTools
{
    [McpServerTool]
    [Description("Analyze C# file for classes and methods")]
    public static string[] AnalyzeFile(
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
}
