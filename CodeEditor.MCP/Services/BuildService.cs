using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;

namespace CodeEditor.MCP.Services;

public class BuildService(IPathService pathService) : IBuildService
{
    static BuildService()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }
    }

    public async Task<string> BuildProject(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        
        try
        {
            var logger = new StringLogger();
            var buildRequest = new BuildRequestData(fullPath, new Dictionary<string, string>(), null, new[] { "Build" }, null);
            var buildManager = BuildManager.DefaultBuildManager;
            
            var result = await Task.Run(() => buildManager.Build(new BuildParameters { Loggers = [logger] }, buildRequest));
            
            return result.OverallResult == BuildResultCode.Success 
                ? $"Build successful\n{logger.GetLog()}" 
                : $"Build failed\n{logger.GetLog()}";
        }
        catch (Exception ex)
        {
            return $"Build failed: {ex.Message}";
        }
    }

    public async Task<string> BuildSolution(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        
        try
        {
            var logger = new StringLogger();
            var buildRequest = new BuildRequestData(fullPath, new Dictionary<string, string>(), null, new[] { "Build" }, null);
            var buildManager = BuildManager.DefaultBuildManager;
            
            var result = await Task.Run(() => buildManager.Build(new BuildParameters { Loggers = [logger] }, buildRequest));
            
            return result.OverallResult == BuildResultCode.Success 
                ? $"Build successful\n{logger.GetLog()}" 
                : $"Build failed\n{logger.GetLog()}";
        }
        catch (Exception ex)
        {
            return $"Build failed: {ex.Message}";
        }
    }

    private class StringLogger : ILogger
    {
        private readonly List<string> _messages = new();
        
        public void Initialize(IEventSource eventSource)
        {
            eventSource.ErrorRaised += (_, e) => _messages.Add($"ERROR: {e.Message}");
            eventSource.WarningRaised += (_, e) => _messages.Add($"WARNING: {e.Message}");
            eventSource.MessageRaised += (_, e) => 
            {
                if (e.Importance == MessageImportance.High && e.Message != null)
                {
                    _messages.Add(e.Message);
                }
            };
        }

        public void Shutdown() { }
        
        public string GetLog() => string.Join("\n", _messages);
        
        public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;
        
        public string Parameters { get; set; } = string.Empty;
    }
}
