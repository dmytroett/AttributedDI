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

        // Keyed services - RegisterAs<T> with key
        AssertContainsKeyedService<IKeyedService, KeyedServiceOne>(services, "key1", ServiceLifetime.Singleton);
        AssertContainsKeyedService<IKeyedService, KeyedServiceTwo>(services, "key2", ServiceLifetime.Singleton);

        // Keyed services - RegisterAsSelf with key
        AssertContainsKeyedService<RegisterAsSelfKeyedTransientService, RegisterAsSelfKeyedTransientService>(services, "transientKey", ServiceLifetime.Transient);
        AssertContainsKeyedService<RegisterAsSelfKeyedSingletonService, RegisterAsSelfKeyedSingletonService>(services, "singletonKey", ServiceLifetime.Singleton);
    }
}