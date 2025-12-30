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

        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<IEmailService>());
        Assert.NotNull(serviceProvider.GetService<ComplicatedSystemFacade>());
        Assert.NotNull(serviceProvider.GetService<IComplicatedSystemFacade>());
        Assert.NotNull(serviceProvider.GetService<ICold>());
        Assert.NotNull(serviceProvider.GetService<IAsyncDisposable>());
        Assert.NotNull(serviceProvider.GetService<IDisposable>());
        Assert.NotNull(serviceProvider.GetService<SimpleService>());
    }
}