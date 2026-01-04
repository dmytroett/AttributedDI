using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace AttributedDI;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to register service modules.
/// </summary>
public static partial class AttributedDiServiceCollectionExtensions
{
    /// <summary>
    /// Adds a service module to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the module to.</param>
    /// <param name="module">The service module to register.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="module"/> is null.</exception>
    public static IServiceCollection AddModule(this IServiceCollection services, IServiceModule module)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(module);

        module.ConfigureServices(services);
        return services;
    }

    /// <summary>
    /// Adds a service module of type <typeparamref name="TModule"/> to the service collection.
    /// </summary>
    /// <typeparam name="TModule">The type of service module to register. Must have a parameterless constructor.</typeparam>
    /// <param name="services">The service collection to add the module to.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    /// <exception cref="MissingMethodException">Thrown when <typeparamref name="TModule"/> does not have a parameterless constructor.</exception>
    public static IServiceCollection AddModule<TModule>(this IServiceCollection services)
        where TModule : IServiceModule, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        var module = new TModule();
        return services.AddModule(module);
    }

    /// <summary>
    /// Adds multiple service modules to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the modules to.</param>
    /// <param name="modules">The service modules to register.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="modules"/> is null.</exception>
    public static IServiceCollection AddModules(this IServiceCollection services, params IServiceModule[] modules)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(modules);

        foreach (var module in modules)
        {
            _ = services.AddModule(module);
        }

        return services;
    }

    /// <summary>
    /// Adds multiple service modules to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the modules to.</param>
    /// <param name="modules">The service modules to register.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="modules"/> is null.</exception>
    public static IServiceCollection AddModules(this IServiceCollection services, IEnumerable<IServiceModule> modules)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(modules);

        foreach (var module in modules)
        {
            _ = services.AddModule(module);
        }

        return services;
    }

    /// <summary>
    /// Adds AttributedDI services to the collection using the provided configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddAttributedDi(this IServiceCollection services)
    {
        return AddAttributedDi(services, _ => { });
    }

    /// <summary>
    /// Adds AttributedDI services to the collection using the provided configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">A callback to configure AttributedDI options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is null.</exception>
    public static IServiceCollection AddAttributedDi(this IServiceCollection services, Action<AttributedDiOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        AttributedDiOptions opts = new();

        configure(opts);

        if (opts.RegisterGeneratedModules)
        {
            AddAttributedDiGeneratedModules(services);
        }

        if (opts.AdditionalModules.Count > 0)
        {
            foreach (var module in opts.AdditionalModules)
            {
                _ = services.AddModule(module);
            }
        }

        if (opts.EnableDeferred)
        {
            // TODO: Lazy<> support but without allocations
        }

        if (opts.EnableOwned)
        {
            // TODO: enable Owned<> support
        }

        if (opts.EnableFactory)
        {
            // TODO: add Func<> support.
        }

        return services;
    }

    static partial void AddAttributedDiGeneratedModules(IServiceCollection services);
}