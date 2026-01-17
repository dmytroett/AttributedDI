using AllModulesRegistration.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.IntegrationTests;

public class AllModulesRegistrationTests
{

    [Fact]
    public void AllServicesRegisteredCorrectly()
    {
        ServiceCollection services = new();

        services.AddAttributedDi();
        using var provider = services.BuildServiceProvider();

        ServiceProviderAssert.Resolves<IMyAmazingService, MyAmazingService>(provider, ServiceLifetime.Transient);
        ServiceProviderAssert.Resolves<IInternalService, InternalService>(provider, ServiceLifetime.Transient);
    }
}