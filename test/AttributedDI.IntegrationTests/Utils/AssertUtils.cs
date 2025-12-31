using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.IntegrationTests.Utils;

public static class AssertUtils
{
    public static void AssertContainsService<TService, TImplementation>(IServiceCollection services, ServiceLifetime expectedLifetime)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService) && d.ImplementationType == typeof(TImplementation));
        Assert.NotNull(descriptor);
        Assert.Equal(expectedLifetime, descriptor.Lifetime);
    }

    public static void AssertDoesNotContainService<TService, TImplementation>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService) && d.ImplementationType == typeof(TImplementation));
        Assert.Null(descriptor);
    }

    public static void AssertContainsKeyedService<TService, TImplementation>(IServiceCollection services, object key, ServiceLifetime expectedLifetime)
    {
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(TService) &&
            d.KeyedImplementationType == typeof(TImplementation) &&
            d.ServiceKey?.Equals(key) == true);
        Assert.NotNull(descriptor);
        Assert.Equal(expectedLifetime, descriptor.Lifetime);
    }
}