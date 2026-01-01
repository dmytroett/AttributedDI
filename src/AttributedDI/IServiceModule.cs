using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI;

/// <summary>
/// Interface for service modules that configure dependency injection registrations.
/// The source generator automatically implements this interface for assemblies with registered types.
/// You can also manually implement this interface to create custom service modules.
/// </summary>
/// <remarks>
/// Generated modules have a virtual <see cref="ConfigureServices"/> method that can be overridden
/// in partial class definitions to add custom registration logic.
/// Use the extension methods in <see cref="AttributedDiServiceCollectionExtensions"/> to register modules.
/// </remarks>
public interface IServiceModule
{
    /// <summary>
    /// Configures services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    void ConfigureServices(IServiceCollection services);
}