namespace CodeEditor.MCP.Services;

public interface ICSharpService
{
    string[] AnalyzeFile(string relativePath);
    void AddMethod(string relativePath, string className, string methodCode);
    void ReplaceMethod(string relativePath, string className, string oldMethodName, string newMethodCode);
    void RemoveMethod(string relativePath, string className, string methodName);
    void AddProperty(string relativePath, string className, string propertyCode);
    void ReplaceProperty(string relativePath, string className, string oldPropertyName, string newPropertyCode);
    void RemoveProperty(string relativePath, string className, string propertyName);
    
    // Interface manipulation methods
    void CreateInterface(string relativePath, string interfaceName, string interfaceCode);
    void AddMethodToInterface(string relativePath, string interfaceName, string methodSignature);
    void ReplaceMethodInInterface(string relativePath, string interfaceName, string oldMethodName, string newMethodSignature);
    void RemoveMethodFromInterface(string relativePath, string interfaceName, string methodName);
}
