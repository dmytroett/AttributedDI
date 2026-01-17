using Company.TeamName.Project.API;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.IntegrationTests;

public class CoreScenariosTests
{
    [Fact]
    public void ServiceRegistrationTests()
    {
        var services = new ServiceCollection();

        services.AddCompanyTeamNameProjectAPI();
        using var provider = services.BuildServiceProvider();

        // RegisterAsSelf implicit transient
        Resolves<RegisterAsSelfTransientImplicitService, RegisterAsSelfTransientImplicitService>(provider, ServiceLifetime.Transient);

        // RegisterAsSelf singleton
        Resolves<RegisterAsSelfSingletonService, RegisterAsSelfSingletonService>(provider, ServiceLifetime.Singleton);

        // RegisterAsSelf scoped
        Resolves<RegisterAsSelfScopedService, RegisterAsSelfScopedService>(provider, ServiceLifetime.Scoped);

        // RegisterAs interface
        Resolves<IRegisterAsInterfaceService, RegisterAsInterfaceScopedService>(provider, ServiceLifetime.Scoped);

        // RegisterAsImplementedInterfaces should register concrete but not IDisposable/IAsyncDisposable
        Resolves<IFirstService, MultiInterfaceSingletonService>(provider, ServiceLifetime.Singleton);
        Resolves<ISecondService, MultiInterfaceSingletonService>(provider, ServiceLifetime.Singleton);
        DoesNotResolve<IDisposable>(provider);
        DoesNotResolve<IAsyncDisposable>(provider);

        // Lifetime-only attribute registers as self
        Resolves<LifetimeOnlyTransientService, LifetimeOnlyTransientService>(provider, ServiceLifetime.Transient);

        // Keyed services - RegisterAs<T> with key
        ResolvesKeyed<IKeyedService, KeyedServiceOne>(provider, "key1", ServiceLifetime.Singleton);
        ResolvesKeyed<IKeyedService, KeyedServiceTwo>(provider, "key2", ServiceLifetime.Singleton);

        // Keyed services - RegisterAsSelf with key
        ResolvesKeyed<RegisterAsSelfKeyedTransientService, RegisterAsSelfKeyedTransientService>(provider, "transientKey", ServiceLifetime.Transient);
        ResolvesKeyed<RegisterAsSelfKeyedSingletonService, RegisterAsSelfKeyedSingletonService>(provider, "singletonKey", ServiceLifetime.Singleton);
    }
}