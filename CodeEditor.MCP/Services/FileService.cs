using System.IO.Abstractions;

namespace CodeEditor.MCP.Services;

public class FileService(IFileSystem fileSystem, IPathService pathService) : IFileService
{
    public string[] ListFiles(string relativePath = "")
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var files = fileSystem.Directory.GetFileSystemEntries(fullPath, "*", SearchOption.AllDirectories);
        var baseDir = pathService.GetBaseDirectory();
        
        var relativePaths = files.Select(f => 
        {
            var relPath = Path.GetRelativePath(baseDir, f);
            // Normalize path separators to forward slashes for gitignore compatibility
            relPath = relPath.Replace(Path.DirectorySeparatorChar, '/');
            
            // Append trailing slash for directories to match gitignore directory patterns
            if (fileSystem.Directory.Exists(f))
            {
                relPath += "/";
            }
            
            return relPath;
        });
        
        return pathService.FilterIgnored(relativePaths).ToArray();
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
    
    public void DeleteFile(string relativePath)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        if (fileSystem.File.Exists(fullPath))
            fileSystem.File.Delete(fullPath);
        else if (fileSystem.Directory.Exists(fullPath))
            fileSystem.Directory.Delete(fullPath, true);
    }
    
    public string[] SearchFiles(string searchText, string relativePath = "")
    {
        var files = ListFiles(relativePath).Where(f => 
        {
            // Remove trailing slash for file existence check
            var pathForCheck = f.TrimEnd('/');
            return fileSystem.File.Exists(pathService.GetFullPath(pathForCheck));
        });
        var matches = new List<string>();
        
        foreach (var file in files)
        {
            try
            {
                // Remove trailing slash for file reading
                var pathForReading = file.TrimEnd('/');
                var content = ReadFile(pathForReading);
                if (content.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    matches.Add(file);
            }
            catch { }
        }
        
        return matches.ToArray();
    }
    
    public void CopyFile(string sourceRelativePath, string destinationRelativePath)
    {
        var sourcePath = pathService.GetFullPath(sourceRelativePath);
        var destPath = pathService.GetFullPath(destinationRelativePath);
        
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
    
    public void MoveFile(string sourceRelativePath, string destinationRelativePath)
    {
        var sourcePath = pathService.GetFullPath(sourceRelativePath);
        var destPath = pathService.GetFullPath(destinationRelativePath);
        
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
