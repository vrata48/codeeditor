using System.Text.Json;

namespace CodeEditor.MCP.Services;

public class ToolLoggingService : IToolLoggingService
{
    private readonly string _logDirectory;
    private readonly IPathService _pathService;

    public ToolLoggingService(IPathService pathService)
    {
        _pathService = pathService;
        _logDirectory = Path.Combine(_pathService.GetBaseDirectory(), ".mcp-logs");
        Directory.CreateDirectory(_logDirectory);
    }
public void LogFailedToolCall(string toolName, string methodName, object? request, Exception exception)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            ToolName = toolName,
            MethodName = methodName,
            Request = SanitizeRequest(request),
            Exception = new
            {
                Type = exception.GetType().FullName,
                Message = exception.Message
            }
        };

        try
        {
            var logFile = Path.Combine(_logDirectory, $"failed-tools-{DateTime.UtcNow:yyyy-MM-dd}.json");
            
            List<object> logEntries;
            
            // Read existing log entries if file exists
            if (File.Exists(logFile))
            {
                var existingContent = File.ReadAllText(logFile);
                if (!string.IsNullOrWhiteSpace(existingContent))
                {
                    logEntries = JsonSerializer.Deserialize<List<object>>(existingContent) ?? new List<object>();
                }
                else
                {
                    logEntries = new List<object>();
                }
            }
            else
            {
                logEntries = new List<object>();
            }
            
            // Add new entry
            logEntries.Add(logEntry);
            
            // Write back as JSON array
            var logJson = JsonSerializer.Serialize(logEntries, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(logFile, logJson);
        }
        catch
        {
            // Swallow logging errors to avoid interfering with the main application
        }
    }     private static object? SanitizeRequest(object? request)
    {
        if (request == null) return null;

        // Convert to dictionary for easier manipulation
        var json = JsonSerializer.Serialize(request);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        if (dict == null) return request;

        // Truncate large content to avoid huge log files
        foreach (var key in dict.Keys.ToList())
        {
            if (key.Equals("content", StringComparison.OrdinalIgnoreCase) && 
                dict[key].ValueKind == JsonValueKind.String)
            {
                var content = dict[key].GetString();
                if (content != null && content.Length > 200)
                {
                    dict[key] = JsonSerializer.SerializeToElement(content[..200] + "... [truncated]");
                }
            }
        }

        return dict;
    }
}
