using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Abstractions;
using CodeEditor.MCP;
using CodeEditor.MCP.Services;

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync(async options =>
    {
        var builder = Host.CreateApplicationBuilder(args);

        var baseDirectory = Path.GetFullPath(options.Directory);

        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IPathService>(provider => new PathService(baseDirectory));
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<ICSharpService, CSharpService>();
        builder.Services.AddSingleton<IBuildService, BuildService>();

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        var app = builder.Build();
        await app.RunAsync();
    });