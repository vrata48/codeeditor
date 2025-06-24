using CommandLine;

namespace CodeEditor.MCP;

public class Options
{
[Option('d', "directory", Required = true, HelpText = "Base directory for file operations.")]
    public string Directory { get; set; } = string.Empty;
}