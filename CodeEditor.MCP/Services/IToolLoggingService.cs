namespace CodeEditor.MCP.Services;

public interface IToolLoggingService
{
    void LogFailedToolCall(string toolName, string methodName, object? request, Exception exception);
}
