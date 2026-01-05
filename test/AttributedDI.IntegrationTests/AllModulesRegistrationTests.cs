using Microsoft.Extensions.DependencyInjection;
using AllModulesRegistration.Core;

namespace AttributedDI.IntegrationTests;

public class AllModulesRegistrationTests
{

    [Fact]
    public void AllServicesRegisteredCorrectly()
    {
        ServiceCollection services = new ();

        services.AddAttributedDi();

        AssertContainsService<IMyAmazingService, MyAmazingService>(services, ServiceLifetime.Transient);
        AssertContainsService<IInternalService, InternalService>(services, ServiceLifetime.Transient);
    }
}
