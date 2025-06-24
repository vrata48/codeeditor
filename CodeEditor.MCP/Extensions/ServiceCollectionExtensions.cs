using Microsoft.Extensions.DependencyInjection;

namespace CodeEditor.MCP.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a singleton service that will be intercepted by AspectInjector at compile time.
    /// This is a wrapper around AddSingleton since AspectInjector handles interception at compile time.
    /// </summary>
    public static IServiceCollection AddInterceptedSingleton<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        return services.AddSingleton<TInterface, TImplementation>();
    }

    /// <summary>
    /// Adds a singleton service that will be intercepted by AspectInjector at compile time.
    /// This is a wrapper around AddSingleton since AspectInjector handles interception at compile time.
    /// </summary>
    public static IServiceCollection AddInterceptedSingleton<TInterface>(
        this IServiceCollection services,
        Func<IServiceProvider, TInterface> factory)
        where TInterface : class
    {
        return services.AddSingleton(factory);
    }

    /// <summary>
    /// Adds a singleton service that will be intercepted by AspectInjector at compile time.
    /// This is a wrapper around AddSingleton since AspectInjector handles interception at compile time.
    /// </summary>
    public static IServiceCollection AddInterceptedSingleton<TService>(
        this IServiceCollection services)
        where TService : class
    {
        return services.AddSingleton<TService>();
    }
}
