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
        AssertContainsService<RegisterAsSelfTransientImplicitService, RegisterAsSelfTransientImplicitService>(provider, ServiceLifetime.Transient);

        // RegisterAsSelf singleton
        AssertContainsService<RegisterAsSelfSingletonService, RegisterAsSelfSingletonService>(provider, ServiceLifetime.Singleton);

        // RegisterAsSelf scoped
        AssertContainsService<RegisterAsSelfScopedService, RegisterAsSelfScopedService>(provider, ServiceLifetime.Scoped);

        // RegisterAs interface
        AssertContainsService<IRegisterAsInterfaceService, RegisterAsInterfaceScopedService>(provider, ServiceLifetime.Scoped);

        // RegisterAsImplementedInterfaces should register concrete but not IDisposable/IAsyncDisposable
        AssertContainsService<IFirstService, MultiInterfaceSingletonService>(provider, ServiceLifetime.Singleton);
        AssertContainsService<ISecondService, MultiInterfaceSingletonService>(provider, ServiceLifetime.Singleton);
        AssertDoesNotContainService<IDisposable>(provider);
        AssertDoesNotContainService<IAsyncDisposable>(provider);

        // Lifetime-only attribute registers as self
        AssertContainsService<LifetimeOnlyTransientService, LifetimeOnlyTransientService>(provider, ServiceLifetime.Transient);

        // Keyed services - RegisterAs<T> with key
        AssertContainsKeyedService<IKeyedService, KeyedServiceOne>(provider, "key1", ServiceLifetime.Singleton);
        AssertContainsKeyedService<IKeyedService, KeyedServiceTwo>(provider, "key2", ServiceLifetime.Singleton);

        // Keyed services - RegisterAsSelf with key
        AssertContainsKeyedService<RegisterAsSelfKeyedTransientService, RegisterAsSelfKeyedTransientService>(provider, "transientKey", ServiceLifetime.Transient);
        AssertContainsKeyedService<RegisterAsSelfKeyedSingletonService, RegisterAsSelfKeyedSingletonService>(provider, "singletonKey", ServiceLifetime.Singleton);
    }
}