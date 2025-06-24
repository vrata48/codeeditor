# CodeEditor.MCP Requirements

## Core Requirements

### R-001: Gitignore Filtering
**All file operations MUST respect gitignore patterns**
- Any operation that returns or processes files must filter through gitignore
- Files and directories matching gitignore patterns should be excluded from all results
- Standard gitignore syntax must be supported including negation patterns (!pattern)

### R-002: File System Abstraction
**All file operations MUST use the IFileSystem abstraction**
- Services must work with both real file system and mock file system for testing
- No direct System.IO calls in service implementations
- Consistent behavior across different file system implementations

### R-003: Path Consistency
**All file paths MUST be normalized and consistent**
- Use forward slashes for path separators
- Relative paths only, no absolute paths in public APIs
- Consistent path format across all operations

### R-004: Error Resilience
**Services MUST handle errors gracefully**
- Continue processing when individual files fail
- No crashes due to malformed patterns or inaccessible files
- Appropriate fallback behavior when external dependencies fail

### R-005: Separation of Concerns
**Clear separation between file operations and path logic**
- FileService handles file I/O operations
- PathService handles path resolution and gitignore logic
- Services should not mix responsibilities

### R-006: Testing Compatibility
**All functionality MUST be testable**
- Services must work with MockFileSystem for unit testing
- Dependency injection for all external dependencies
- Predictable behavior for test scenarios

### R-007: Cross-Platform Support
**Services MUST work across operating systems**
- Handle Windows, macOS, and Linux path differences
- No platform-specific file system assumptions
- Consistent behavior regardless of underlying OS

## Design Principles

### P-001: Gitignore First
**Gitignore filtering is the primary concern, not a secondary feature**
- All file listing operations start with gitignore filtering
- Other features (search, formatting) are applied after gitignore filtering
- When in doubt, respect gitignore rules

### P-002: Fail Safe
**When operations fail, default to safe behavior**
- If gitignore patterns are malformed, continue with available patterns
- If file reading fails, skip the file and continue
- Prefer returning fewer results than incorrect results

### P-003: Interface Consistency
**All similar operations should behave consistently**
- Same path normalization across all methods
- Same error handling patterns across all services
- Same gitignore behavior across all file operations

### P-004: Testability First
**Design for testing from the beginning**
- All external dependencies must be mockable
- Services should work identically with real and mock implementations
- Clear separation of concerns to enable focused unit testing
