using AspectInjector.Broker;
using CodeEditor.MCP.Services;
using System.Reflection;

namespace CodeEditor.MCP.Aspects;

[Aspect(Scope.Global)]
[Injection(typeof(ToolLoggingAspect))]
public class ToolLoggingAspect : Attribute
{
    private static IServiceProvider? _serviceProvider;
    
    // This will be called by the DI container to set the service provider
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [Advice(Kind.Around, Targets = Target.Method)]
    public object LogToolExecution(
        [Argument(Source.Target)] Func<object[], object> target,
        [Argument(Source.Arguments)] object[] arguments,
        [Argument(Source.Name)] string methodName,
        [Argument(Source.Type)] Type declaringType)
    {
        try
        {
            return target(arguments);
        }
        catch (Exception ex)
        {
            LogFailure(methodName, declaringType, arguments, ex);
            throw; // Re-throw to maintain original behavior
        }
    }

    private static void LogFailure(string methodName, Type declaringType, object[] arguments, Exception exception)
    {
        try
        {
            var toolName = GetToolName(declaringType);
            var parameters = ExtractUserParameters(methodName, arguments);
            var loggingService = GetLoggingService();
            
            loggingService?.LogFailedToolCall(toolName, methodName, parameters, exception);
        }
        catch
        {
            // Swallow logging errors to avoid interfering with main execution
        }
    }

    private static string GetToolName(Type declaringType)
    {
        var name = declaringType.Name;
        
        // Remove "Tools" suffix for cleaner tool names
        if (name.EndsWith("Tools"))
            name = name[..^5];
            
        return name;
    }

    private static object? ExtractUserParameters(string methodName, object[] arguments)
    {
        if (arguments == null || arguments.Length == 0)
            return null;

        var parameters = new Dictionary<string, object?>();
        var parameterNames = GetParameterNames(methodName, arguments);
        
        for (int i = 0; i < arguments.Length; i++)
        {
            var arg = arguments[i];
            
            // Skip service parameters (interfaces starting with 'I' and containing 'Service')
            if (arg?.GetType().IsInterface == true && 
                arg.GetType().Name.StartsWith("I") && 
                arg.GetType().Name.Contains("Service"))
            {
                continue;
            }
            
            // Add user parameters with meaningful names
            var paramName = parameterNames.Length > i ? parameterNames[i] : $"param{i}";
            parameters[paramName] = SanitizeParameter(arg);
        }
        
        return parameters.Count > 0 ? parameters : null;
    }

    private static string[] GetParameterNames(string methodName, object[] arguments)
    {
        // In a real implementation, you could use reflection to get actual parameter names
        // For now, we'll use generic names
        return arguments.Select((_, i) => $"param{i}").ToArray();
    }

    private static object? SanitizeParameter(object? parameter)
    {
        if (parameter is string str && str.Length > 200)
        {
            return str[..200] + "... [truncated]";
        }
        return parameter;
    }

    private static IToolLoggingService? GetLoggingService()
    {
        try
        {
            // Try to get from service provider if available
            if (_serviceProvider != null)
            {
                return _serviceProvider.GetService(typeof(IToolLoggingService)) as IToolLoggingService;
            }
            
            // Fallback: create a simple instance
            var tempDir = Path.GetTempPath();
            return new ToolLoggingService(new PathService(tempDir));
        }
        catch
        {
            return null;
        }
    }
}
