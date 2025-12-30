using Company.TeamName.Project.API;
using Company.TeamName.Project.API.Generated;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.IntegrationTests;

public class RegistrationsTestsWithStandardAssemblyName
{
    [Fact]
    public void ServicesRegisteredCorrectly()
    {
        var services = new ServiceCollection();

        services.AddCompanyTeamNameProjectAPI();

        // RegisterAsSelf implicit transient
        AssertContainsService<RegisterAsSelfTransientImplicitService, RegisterAsSelfTransientImplicitService>(services, ServiceLifetime.Transient);

        // RegisterAsSelf singleton
        AssertContainsService<RegisterAsSelfSingletonService, RegisterAsSelfSingletonService>(services, ServiceLifetime.Singleton);

        // RegisterAsSelf scoped
        AssertContainsService<RegisterAsSelfScopedService, RegisterAsSelfScopedService>(services, ServiceLifetime.Scoped);

        // RegisterAs interface
        AssertContainsService<IRegisterAsInterfaceService, RegisterAsInterfaceScopedService>(services, ServiceLifetime.Scoped);

        // RegisterAsImplementedInterfaces should register concrete but not IDisposable/IAsyncDisposable
        AssertContainsService<IFirstService, MultiInterfaceSingletonService>(services, ServiceLifetime.Singleton);
        AssertContainsService<ISecondService, MultiInterfaceSingletonService>(services, ServiceLifetime.Singleton);
        AssertDoesNotContainService<IDisposable, MultiInterfaceSingletonService>(services);
        AssertDoesNotContainService<IAsyncDisposable, MultiInterfaceSingletonService>(services);

        // Lifetime-only attribute registers as self
        AssertContainsService<LifetimeOnlyTransientService, LifetimeOnlyTransientService>(services, ServiceLifetime.Transient);
    }

    private static void AssertContainsService<TService, TImplementation>(IServiceCollection services, ServiceLifetime expectedLifetime)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService) && d.ImplementationType == typeof(TImplementation));
        Assert.NotNull(descriptor);
        Assert.Equal(expectedLifetime, descriptor.Lifetime);
    }

    private static void AssertDoesNotContainService<TService, TImplementation>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService) && d.ImplementationType == typeof(TImplementation));
        Assert.Null(descriptor);
    }
}