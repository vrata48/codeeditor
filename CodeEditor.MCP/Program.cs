using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Abstractions;
using CodeEditor.MCP;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Aspects;
using Microsoft.Extensions.Logging;

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync(async options =>
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.None);

        var baseDirectory = Path.GetFullPath(options.Directory);

        // Register core dependencies
        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IPathService>(_ => new PathService(baseDirectory));
        builder.Services.AddSingleton<IFileFilterService>(_ => new FileFilterService(
            new PathService(baseDirectory), null));
        
        // Register the tool logging service (used by AspectInjector)
        builder.Services.AddSingleton<IToolLoggingService, ToolLoggingService>();

        // Register services normally - AspectInjector will handle the interception at compile time
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<ICSharpService, CSharpService>();
        builder.Services.AddSingleton<IDotNetService, DotNetService>();
        builder.Services.AddSingleton<ICSharpFormattingService, CSharpFormattingService>();
        builder.Services.AddSingleton<IFileAnalysisService, FileAnalysisService>();

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        var host = builder.Build();
        
        // Provide the service provider to the logging aspect
        ToolLoggingAspect.SetServiceProvider(host.Services);

        await host.RunAsync();
    });
