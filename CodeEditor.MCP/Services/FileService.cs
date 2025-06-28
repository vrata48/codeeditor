using System.IO.Abstractions;
using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services;

public class FileService(IFileSystem fileSystem, IPathService pathService) : IFileService
{
    public string[] ListFiles(string relativePath = ".", string? filter = null)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var entries = fileSystem.Directory.GetFileSystemEntries(fullPath, "*", SearchOption.AllDirectories);
        
        // Convert to relative paths with proper normalization
        var relativePaths = entries.Select(entry => 
        {
            var relPath = pathService.GetRelativePath(entry);
            // Add trailing slash for directories
            if (fileSystem.Directory.Exists(entry) && !relPath.EndsWith("/"))
            {
                relPath += "/";
            }
            return relPath;
        });
        
        // Apply gitignore filtering
        var gitignoreFiltered = pathService.FilterIgnored(relativePaths);
        
        // Apply additional filter if provided
        if (!string.IsNullOrEmpty(filter))
        {
            return pathService.FilterByPatterns(gitignoreFiltered, filter).ToArray();
        }
        
        return gitignoreFiltered.ToArray();
    }

    public string ReadFile(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        return fileSystem.File.ReadAllText(fullPath);
    }
    
    public void WriteFile(string relativePath, string content)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        fileSystem.File.WriteAllText(fullPath, content);
    }
    
    public void DeleteFiles(string[] relativePaths)
    {
        foreach (var relativePath in relativePaths)
        {
            var fullPath = pathService.GetFullPath(relativePath);
            if (fileSystem.File.Exists(fullPath))
                fileSystem.File.Delete(fullPath);
            else if (fileSystem.Directory.Exists(fullPath))
                fileSystem.Directory.Delete(fullPath, true);
        }
    }

    public string[] SearchFiles(string searchText, string relativePath = ".", string? filter = null)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var allFiles = fileSystem.Directory.GetFileSystemEntries(fullPath, "*", SearchOption.AllDirectories);
        var results = new List<string>();
        
        foreach (var fullFilePath in allFiles)
        {
            // Skip directories
            if (fileSystem.Directory.Exists(fullFilePath))
                continue;
                
            // Check if this file should be included (gitignore filtering)
            var relativeFilePath = pathService.GetRelativePath(fullFilePath);
            if (pathService.ShouldIgnore(relativeFilePath))
                continue;
                
            // Apply additional filter if provided
            if (!string.IsNullOrEmpty(filter) && !pathService.MatchesFilter(relativeFilePath, filter))
                continue;
                
            try
            {
                var content = fileSystem.File.ReadAllText(fullFilePath);
                if (content.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(relativeFilePath.Replace('\\', '/'));
                }
            }
            catch
            {
                // Skip files that can't be read
            }
        }
        
        return results.ToArray();
    }

    public void CopyFiles(FileOperation[] operations)
    {
        foreach (var operation in operations)
        {
            var sourcePath = pathService.GetFullPath(operation.Source);
            var destPath = pathService.GetFullPath(operation.Destination);
            
            if (fileSystem.File.Exists(sourcePath))
            {
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                fileSystem.File.Copy(sourcePath, destPath, true);
            }
            else if (fileSystem.Directory.Exists(sourcePath))
            {
                CopyDirectory(sourcePath, destPath);
            }
        }
    }
    
    public void MoveFiles(FileOperation[] operations)
    {
        foreach (var operation in operations)
        {
            var sourcePath = pathService.GetFullPath(operation.Source);
            var destPath = pathService.GetFullPath(operation.Destination);
            
            if (fileSystem.File.Exists(sourcePath))
            {
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                fileSystem.File.Move(sourcePath, destPath);
            }
            else if (fileSystem.Directory.Exists(sourcePath))
            {
                fileSystem.Directory.Move(sourcePath, destPath);
            }
        }
    }
    
    private void CopyDirectory(string sourceDir, string destDir)
    {
        fileSystem.Directory.CreateDirectory(destDir);
        
        foreach (var file in fileSystem.Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            fileSystem.File.Copy(file, destFile, true);
        }
        
        foreach (var dir in fileSystem.Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }
}
