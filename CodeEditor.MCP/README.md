# MCP File & C# Tools - Requirements

## What We're Building
MCP server tools for file operations + C# build/analyze. Extensible foundation.

## MCP Tools to Implement

### File Operations
- **list_files** - show files/folders
- **read_file** - get file content  
- **write_file** - save file content
- **delete_file** - remove files/folders
- **search_files** - find text in files
- **copy_file** - copy file/folder
- **move_file** - move/rename file/folder

### C# Operations  
- **build_project** - compile project/solution
- **analyze_file** - list classes/methods in file
- **add_method** - add method to class
- **replace_method** - replace existing method
- **remove_method** - remove method from class

## Rules
- All file paths are relative to base directory specified via CLI args
- All operations respect .gitignore
- Use NuGet for everything possible
- Classes under 50 lines
- No comments
- Keep API simple - no overthinking

## Key Libraries
```xml
<PackageReference Include="Ignore" Version="0.2.1" />
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" />
<PackageReference Include="Microsoft.Build" Version="17.8.3" />
<PackageReference Include="System.IO.Abstractions" Version="21.1.3" />
<PackageReference Include="CommandLineParser" Version="2.9.1" />
<PackageReference Include="ModelContextProtocol" Version="0.2.0-preview.3" />
```

## MCP Tool Definitions
Each tool returns structured JSON responses and accepts parameters as defined by MCP protocol.
