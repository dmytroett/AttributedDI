using CustomRegistrationMethodName;
using Microsoft.Extensions.DependencyInjection;
using MyUnbelievableNamespace;

namespace AttributedDI.IntegrationTests;

public class CustomModuleNameTests
{
    [Fact]
    public void ExtensionMethod()
    {
        var services = new ServiceCollection();

        services.AddMyAmazingCustomServices();
        using var provider = services.BuildServiceProvider();

        ServiceProviderAssert.Resolves<AliasedAssemblyService, AliasedAssemblyService>(provider, ServiceLifetime.Scoped);
    }

    [Fact]
    public void DirectModuleRegistration()
    {
        var services = new ServiceCollection();

        var module = new MyIncredibleCustomModule();
        module.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        ServiceProviderAssert.Resolves<AliasedAssemblyService, AliasedAssemblyService>(provider, ServiceLifetime.Scoped);
    }
}