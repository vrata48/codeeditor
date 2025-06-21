namespace CodeEditor.MCP.Services;

public interface ICSharpService
{
    string[] AnalyzeFile(string relativePath);
    void AddMethod(string relativePath, string className, string methodCode);
    void ReplaceMethod(string relativePath, string className, string oldMethodName, string newMethodCode);
    void RemoveMethod(string relativePath, string className, string methodName);
}
