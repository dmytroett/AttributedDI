using CustomRegistrationMethodName;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.IntegrationTests;

public class CustomModuleNameTests
{
    [Fact]
    public void ExtensionMethod()
    {
        var services = new ServiceCollection();

        services.AddMyAmazingCustomServices();

        AssertContainsService<AliasedAssemblyService, AliasedAssemblyService>(services, ServiceLifetime.Scoped);
    }

    [Fact]
    public void DirectModuleRegistration()
    {
        var services = new ServiceCollection();

        var module = new MyIncredibleCustomModule();
        module.ConfigureServices(services);

        AssertContainsService<AliasedAssemblyService, AliasedAssemblyService>(services, ServiceLifetime.Scoped);
    }
}