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
        ServiceProviderAssert.Resolves<RegisterAsSelfTransientImplicitService, RegisterAsSelfTransientImplicitService>(provider, ServiceLifetime.Transient);

        // RegisterAsSelf singleton
        ServiceProviderAssert.Resolves<RegisterAsSelfSingletonService, RegisterAsSelfSingletonService>(provider, ServiceLifetime.Singleton);

        // RegisterAsSelf scoped
        ServiceProviderAssert.Resolves<RegisterAsSelfScopedService, RegisterAsSelfScopedService>(provider, ServiceLifetime.Scoped);

        // RegisterAs interface
        ServiceProviderAssert.Resolves<IRegisterAsInterfaceService, RegisterAsInterfaceScopedService>(provider, ServiceLifetime.Scoped);

        // RegisterAsImplementedInterfaces should register concrete but not IDisposable/IAsyncDisposable
        ServiceProviderAssert.Resolves<IFirstService, MultiInterfaceSingletonService>(provider, ServiceLifetime.Singleton);
        ServiceProviderAssert.Resolves<ISecondService, MultiInterfaceSingletonService>(provider, ServiceLifetime.Singleton);
        ServiceProviderAssert.DoesNotResolve<IDisposable>(provider);
        ServiceProviderAssert.DoesNotResolve<IAsyncDisposable>(provider);

        // Lifetime-only attribute registers as self
        ServiceProviderAssert.Resolves<LifetimeOnlyTransientService, LifetimeOnlyTransientService>(provider, ServiceLifetime.Transient);

        // Keyed services - RegisterAs<T> with key
        ServiceProviderAssert.ResolvesKeyed<IKeyedService, KeyedServiceOne>(provider, "key1", ServiceLifetime.Singleton);
        ServiceProviderAssert.ResolvesKeyed<IKeyedService, KeyedServiceTwo>(provider, "key2", ServiceLifetime.Singleton);

        // Keyed services - RegisterAsSelf with key
        ServiceProviderAssert.ResolvesKeyed<RegisterAsSelfKeyedTransientService, RegisterAsSelfKeyedTransientService>(provider, "transientKey", ServiceLifetime.Transient);
        ServiceProviderAssert.ResolvesKeyed<RegisterAsSelfKeyedSingletonService, RegisterAsSelfKeyedSingletonService>(provider, "singletonKey", ServiceLifetime.Singleton);
    }
}