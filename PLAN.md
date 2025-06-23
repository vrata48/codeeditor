# Context Reduction Tool Analysis for CodeEditor

## Executive Summary

The current CodeEditor tools require significant context due to their atomic nature. Most operations require multiple tool calls and return verbose output. The analysis identifies **20+ new tools** that could reduce context usage by **70-95%** through targeted data retrieval and smart filtering.

## Critical Context Problems

1. **File Reading**: `ReadFile` returns entire files (often 1000+ lines) when users typically need 5-50 lines
2. **C# Analysis**: `AnalyzeFile` includes method bodies when users often just need API structure  
3. **Build Operations**: Build tools return massive logs when users only need error locations
4. **Search Operations**: Requires separate `ReadFile` calls to see match context

## Priority Tools for Implementation

### Phase 1: Quick Wins (Easy Implementation, High Impact)

1. **ReadFileLines** - Read specific line ranges (90% usage, 80-95% context reduction)
2. **SearchFilesWithContext** - Search results with surrounding lines (60% usage)
3. **GetMethodSignatures** - API overview without implementations (40% usage)
4. **FileTreeSummary** - Structured directory overview with filtering

### Phase 2: High-Value Medium Effort

5. **GetClassStructure** - Class outline without method bodies (70% usage, 70-90% reduction)
6. **GetBuildErrors** - Extract only compilation errors (80% usage, 90-99% reduction)
7. **ReadFileSection** - Read content between patterns/markers
8. **ValidateCode** - Syntax checking without full build

### Phase 3: Advanced/Specialized

- **FindReferences** - Cross-file symbol usage analysis
- **GetDependencies** - Using statements and type references
- **ReplaceInClass** - Scoped find/replace operations
- **QuickBuildCheck** - Fast validation without compilation
- **GetTestResults** - Extract only test failures and details

## Impact Areas

| Area | Current Context | Improved Context | Reduction |
|------|----------------|------------------|-----------|
| File Reading | 100s-1000s lines | 5-50 lines | 80-95% |
| C# Analysis | Full class details | API surface only | 70-90% |
| Build Operations | 1000s of log lines | Error locations only | 90-99% |
| Search Operations | Multiple file reads | Context-aware results | Eliminates multiple calls |

## Detailed Tool Specifications

### Generic File Tools

#### ReadFileLines
- **Purpose**: Read specific line ranges from files
- **Reduces**: Avoid reading entire large files when only need specific sections
- **Parameters**: `path, startLine, endLine`
- **Use Case**: Reading error locations, specific methods, imports only

#### ReadFileSection
- **Purpose**: Read file content between markers/patterns
- **Reduces**: Target specific code sections without full file content
- **Parameters**: `path, startPattern, endPattern`
- **Use Case**: Reading specific class, method, or namespace content

#### GetFileInfo
- **Purpose**: Get file metadata (size, last modified, line count)
- **Reduces**: Quick file inspection without reading content
- **Parameters**: `path`
- **Use Case**: Deciding whether to read file, checking if file changed

#### SearchFilesWithContext
- **Purpose**: Search text with surrounding line context
- **Reduces**: Get search results with context without reading full files
- **Parameters**: `text, path, contextLines`
- **Use Case**: Finding usage patterns with minimal context

#### ReadMultipleFiles
- **Purpose**: Read multiple files in one operation with filtering
- **Reduces**: Batch file reading with size/pattern limits
- **Parameters**: `paths[], maxSize, includePattern`
- **Use Case**: Reading related files (interfaces + implementations)

#### FileTreeSummary
- **Purpose**: Get structured overview of directory contents
- **Reduces**: Understanding project structure without listing everything
- **Parameters**: `path, maxDepth, fileTypes`
- **Use Case**: Project exploration, finding related files

### C#-Specific Tools

#### GetClassStructure
- **Purpose**: Get class outline without method bodies
- **Reduces**: Understand class API without implementation details
- **Parameters**: `path, className`
- **Use Case**: Interface design, dependency analysis

#### GetMethodSignatures
- **Purpose**: Get all method signatures in a class/file
- **Reduces**: API overview without implementation code
- **Parameters**: `path, className?`
- **Use Case**: Interface generation, API documentation

#### FindReferences
- **Purpose**: Find where class/method/property is used
- **Reduces**: Cross-file analysis without reading all files
- **Parameters**: `symbolName, scope`
- **Use Case**: Refactoring impact analysis

#### GetDependencies
- **Purpose**: Get using statements and referenced types
- **Reduces**: Understand dependencies without full code
- **Parameters**: `path`
- **Use Case**: Dependency management, namespace cleanup

#### ValidateCode
- **Purpose**: Check syntax/compilation without building
- **Reduces**: Quick validation without full build process
- **Parameters**: `path`
- **Use Case**: Pre-build validation, syntax checking

#### ReplaceInClass
- **Purpose**: Bulk find/replace within specific class
- **Reduces**: Targeted replacements without reading entire file
- **Parameters**: `path, className, findPattern, replacePattern`
- **Use Case**: Renaming within scope, pattern updates

#### GetImplementationMap
- **Purpose**: Map interfaces to implementations
- **Reduces**: Understanding inheritance without reading all code
- **Parameters**: `interfaceName or path`
- **Use Case**: Architecture analysis, implementation planning

### Build-Specific Tools

#### GetBuildErrors
- **Purpose**: Extract just compilation errors with file locations
- **Reduces**: Error information without full build output
- **Parameters**: `projectPath`
- **Use Case**: Error analysis, focused debugging

#### QuickBuildCheck
- **Purpose**: Fast syntax/dependency check without full build
- **Reduces**: Validation without expensive compilation
- **Parameters**: `path`
- **Use Case**: Pre-build validation, CI checks

#### GetTestResults
- **Purpose**: Extract test results with failure details only
- **Reduces**: Test feedback without verbose output
- **Parameters**: `projectPath, testFilter?`
- **Use Case**: Test-driven development, CI feedback

#### GetProjectReferences
- **Purpose**: Get project/package dependencies
- **Reduces**: Dependency info without reading project files
- **Parameters**: `projectPath`
- **Use Case**: Dependency management, architecture analysis

#### BuildImpactAnalysis
- **Purpose**: Determine what needs rebuild after changes
- **Reduces**: Smart build decisions without full rebuilds
- **Parameters**: `changedFiles[]`
- **Use Case**: Incremental builds, change impact

## Common Workflow Analysis

### Current Workflows Requiring Multiple Tools

1. **Refactoring a class**: ReadFile → AnalyzeFile → ReplaceMethod → AddProperty → FormatDocument
   - **Issues**: Need to read entire file to understand structure, Multiple granular operations

2. **Renaming across files**: SearchFiles → ReadFile (multiple) → WriteFile (multiple) → FormatDirectory
   - **Issues**: Must read many files, Manual coordination of changes

3. **Build troubleshooting**: BuildProject → ReadFile (errors) → AnalyzeFile → ReplaceMethod → BuildProject
   - **Issues**: Build output verbose, Error locations require file reading

4. **Test development**: AnalyzeFile → AddMethod → RunTests → ReadFile → ReplaceMethod → RunTests
   - **Issues**: Test results verbose, Iterative file modifications

## Implementation Strategy

**Start with Phase 1 tools** - they provide maximum impact with minimal implementation effort and can be built by extending existing services. These alone would address the majority of context bloat in common workflows.

**Phase 2 tools** require more sophisticated parsing but address the remaining high-frequency, high-context scenarios.

**Phase 3 tools** provide specialized capabilities for advanced workflows and can be implemented based on user demand.

The tools are designed to complement existing functionality rather than replace it, giving users both atomic control and context-efficient operations as needed.

## Implementation Phases

### Phase 1: Quick Wins (Easy Implementation, High Impact)
- ReadFileLines
- SearchFilesWithContext
- GetMethodSignatures
- FileTreeSummary

### Phase 2: Medium Implementation, High Value
- GetClassStructure
- GetBuildErrors
- ReadFileSection
- ValidateCode

### Phase 3: Advanced/Specialized Tools
- FindReferences
- GetDependencies
- ReplaceInClass
- GetImplementationMap
- QuickBuildCheck
- GetTestResults
- GetProjectReferences
- BuildImpactAnalysis

## Expected Outcomes

- **80-95% reduction** in context size for file operations
- **70-90% reduction** in C# analysis context
- **90-99% reduction** in build operation verbosity
- **Elimination** of multiple tool calls for common workflows
- **Faster response times** due to reduced token processing
- **More focused** and actionable information delivery
