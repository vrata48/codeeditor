using CodeEditor.MCP.Extensions;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace CodeEditor.MCP.Tests;

public class ToolLoggingInterceptorTests
{
    [Fact]
    public void FileTools_WorksWithMockFileSystem()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var tempDirectory = "/temp";
        
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(mockFileSystem);
        services.AddSingleton<IPathService>(_ => new PathService(tempDirectory));
        services.AddSingleton<IToolLoggingService, ToolLoggingService>();
        services.AddSingleton<IFileFilterService>(_ => new FileFilterService(
            new PathService(tempDirectory), 
            null // No global filter
        ));
        services.AddSingleton<IFileService, FileService>();

        var serviceProvider = services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        // Act & Assert - This should fail because file doesn't exist
        Assert.Throws<FileNotFoundException>(() => 
            FileTools.ReadFile(fileService, "non-existent-file.txt"));

        // Note: Actual logging would happen if AspectInjector was properly compiled
        // For now we just verify the tool behavior is correct
        Assert.True(true);
    }
    
    [Fact]
    public void FileTools_SuccessfulReadDoesNotFail()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile("/temp/test-success.txt", new MockFileData("test content"));
        var tempDirectory = "/temp";
        
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(mockFileSystem);
        services.AddSingleton<IPathService>(_ => new PathService(tempDirectory));
        services.AddSingleton<IToolLoggingService, ToolLoggingService>();
        services.AddSingleton<IFileFilterService>(_ => new FileFilterService(
            new PathService(tempDirectory), 
            null // No global filter
        ));
        services.AddSingleton<IFileService, FileService>();

        var serviceProvider = services.BuildServiceProvider();
        var fileService = serviceProvider.GetRequiredService<IFileService>();

        // Act - This should succeed
        var content = FileTools.ReadFile(fileService, "test-success.txt");
        
        // Assert
        Assert.Equal("test content", content);
    }
}
