using GenerateExtensions.OptIn;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.GenerateExtensionsOptIn.IntegrationTests;

public class GenerateExtensionsOptInTests
{
    [Fact]
    public void AddAttributedDiRegistersServicesFromLibrary()
    {
        ServiceCollection services = new();

        services.AddAttributedDi();
        using var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<OptInService>();
        var second = provider.GetRequiredService<OptInService>();

        Assert.NotSame(first, second);
    }
}