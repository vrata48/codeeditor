using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Abstractions;
using CodeEditor.MCP;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Services.CodeStructure;
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
        
        // Register the tool logging service (used by AspectInjector)
        builder.Services.AddSingleton<IToolLoggingService, ToolLoggingService>();

        // Register code structure services
        builder.Services.AddSingleton<ICodeStructureCache, CodeStructureCache>();
        builder.Services.AddSingleton<ICodeAnalysisService, CodeAnalysisService>();
        builder.Services.AddSingleton<ICodeGenerationService, CodeGenerationService>();
        builder.Services.AddSingleton<ICodeModificationService, CodeModificationService>();
        builder.Services.AddSingleton<ICodeQueryService, CodeQueryService>();
        builder.Services.AddSingleton<ICodeValidationService, CodeValidationService>();
        builder.Services.AddSingleton<ICodeRefactoringService, CodeRefactoringService>();
        builder.Services.AddSingleton<IBatchOperationsService, BatchOperationsService>();

        // Register main services
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<IDotNetService, DotNetService>();
        builder.Services.AddSingleton<ICSharpFormattingService, CSharpFormattingService>();
        builder.Services.AddSingleton<ICodeStructureService, CodeStructureService>();

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        var host = builder.Build();
        
        // Provide the service provider to the logging aspect
        ToolLoggingAspect.SetServiceProvider(host.Services);

        await host.RunAsync();
    });
