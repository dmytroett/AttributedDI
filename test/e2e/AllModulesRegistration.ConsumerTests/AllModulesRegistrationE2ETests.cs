using AllModulesRegistration.ConsumerCore;
using Microsoft.Extensions.DependencyInjection;

namespace AllModulesRegistration.ConsumerTests;

public class AllModulesRegistrationE2ETests
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
