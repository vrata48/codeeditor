using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Abstractions;
using CodeEditor.MCP;
using CodeEditor.MCP.Services;
using Microsoft.Extensions.Logging;

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync(async options =>
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.None);

        var baseDirectory = Path.GetFullPath(options.Directory);

        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IPathService>(_ => new PathService(baseDirectory));
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<ICSharpService, CSharpService>();
        builder.Services.AddSingleton<IDotNetService, DotNetService>();

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();
    });