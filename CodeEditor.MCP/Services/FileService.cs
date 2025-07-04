﻿using System.IO.Abstractions;
using CodeEditor.MCP.Models;

namespace CodeEditor.MCP.Services;
public class FileService(IFileSystem fileSystem, IPathService pathService) : IFileService
{
    public Models.FileInfo[] ListFiles(string relativePath = ".", string? filter = null)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var entries = fileSystem.Directory.GetFileSystemEntries(fullPath, "*", SearchOption.AllDirectories);
        var fileInfos = new List<Models.FileInfo>();
        foreach (var entry in entries)
        {
            var relPath = pathService.GetRelativePath(entry);
            // Skip directories for file info
            if (fileSystem.Directory.Exists(entry))
                continue;
            // Apply gitignore filtering
            if (pathService.ShouldIgnore(relPath))
                continue;
            // Apply additional filter if provided
            if (!string.IsNullOrEmpty(filter) && !pathService.MatchesFilter(relPath, filter))
                continue;
            try
            {
                var sysFileInfo = fileSystem.FileInfo.New(entry);
                var content = fileSystem.File.ReadAllText(entry);
                var lineCount = content.Split('\n').Length;
                fileInfos.Add(new Models.FileInfo { Name = sysFileInfo.Name, RelativePath = relPath.Replace('\\', '/'), Size = sysFileInfo.Length, LastModified = sysFileInfo.LastWriteTime, Extension = sysFileInfo.Extension, LineCount = lineCount });
            }
            catch
            {
                // Skip files that can't be read
                fileInfos.Add(new Models.FileInfo { Name = Path.GetFileName(entry), RelativePath = relPath.Replace('\\', '/'), Size = -1, // Indicates unreadable
 LastModified = DateTime.MinValue, Extension = Path.GetExtension(entry), LineCount = -1 });
            }
        }

        return fileInfos.ToArray();
    }

    public string ReadFile(string relativePath, int? startLine = null, int? endLine = null)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var content = fileSystem.File.ReadAllText(fullPath);
        // If no line range specified, return entire file
        if (startLine == null && endLine == null)
            return content;
        // Validate line range parameters
        if (startLine < 1)
            throw new ArgumentException("Start line must be 1 or greater", nameof(startLine));
        if (endLine != null && endLine < startLine)
            throw new ArgumentException("End line must be greater than or equal to start line", nameof(endLine));
        var allLines = content.Split('\n');
        // Convert to 0-based indexing
        var startIndex = (startLine ?? 1) - 1;
        var endIndex = (endLine ?? allLines.Length) - 1;
        endIndex = Math.Min(endIndex, allLines.Length - 1);
        if (startIndex >= allLines.Length)
        {
            return $"// Line {startLine} is beyond end of file (file has {allLines.Length} lines)";
        }

        var selectedLines = allLines.Skip(startIndex).Take(endIndex - startIndex + 1);
        var result = string.Join('\n', selectedLines);
        // Add context information when reading a range
        if (startLine != null || endLine != null)
        {
            var prefix = startIndex > 0 ? $"// ... ({startIndex} lines above)\n" : "";
            var suffix = endIndex < allLines.Length - 1 ? $"\n// ... ({allLines.Length - endIndex - 1} lines below)" : "";
            return prefix + result + suffix;
        }

        return result;
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

    public Models.FileInfo[] SearchFiles(string searchText, string relativePath = ".", string? filter = null)
    {
        var fullPath = pathService.GetFullPath(relativePath);
        var allFiles = fileSystem.Directory.GetFileSystemEntries(fullPath, "*", SearchOption.AllDirectories);
        var results = new List<Models.FileInfo>();
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
                    var sysFileInfo = fileSystem.FileInfo.New(fullFilePath);
                    var lineCount = content.Split('\n').Length;
                    results.Add(new Models.FileInfo { Name = sysFileInfo.Name, RelativePath = relativeFilePath.Replace('\\', '/'), Size = sysFileInfo.Length, LastModified = sysFileInfo.LastWriteTime, Extension = sysFileInfo.Extension, LineCount = lineCount });
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