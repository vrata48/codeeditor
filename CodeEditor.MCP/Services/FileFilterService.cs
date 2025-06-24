namespace CodeEditor.MCP.Services;

public class FileFilterService(IPathService pathService, string? globalFilter) : IFileFilterService
{
    public string? GlobalFilter { get; } = globalFilter;

    public bool ShouldInclude(string relativePath)
    {
        // First check gitignore rules
        if (pathService.ShouldIgnore(relativePath))
            return false;

        // Then check global filter patterns
        return pathService.MatchesFilter(relativePath, GlobalFilter);
    }

    public IEnumerable<string> FilterFiles(IEnumerable<string> relativePaths)
    {
        // Apply gitignore filtering first
        var gitignoreFiltered = pathService.FilterIgnored(relativePaths);
        
        // Then apply pattern filtering
        return pathService.FilterByPatterns(gitignoreFiltered, GlobalFilter);
    }
}