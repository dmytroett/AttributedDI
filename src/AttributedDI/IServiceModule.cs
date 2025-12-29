using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI;

/// <summary>
/// Interface for service modules that configure dependency injection registrations.
/// Types implementing this interface and marked with <see cref="RegisterModuleAttribute"/> 
/// will have their <see cref="ConfigureServices"/> method called during service registration.
/// </summary>
public interface IServiceModule
{
    /// <summary>
    /// Configures services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    void ConfigureServices(IServiceCollection services);
}