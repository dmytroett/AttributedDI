using Microsoft.Extensions.DependencyInjection;

namespace AllModulesRegistration.ConsumerTests;

public static class ServiceProviderAssert
{
    public static void Resolves<TService, TImplementation>(IServiceProvider provider, ServiceLifetime expectedLifetime)
        where TService : notnull
    {
        AssertServiceLifetime<TService, TImplementation>(provider, expectedLifetime, key: null);
    }

    private static void AssertServiceLifetime<TService, TImplementation>(
        IServiceProvider provider,
        ServiceLifetime expectedLifetime,
        object? key)
        where TService : notnull
    {
        switch (expectedLifetime)
        {
            case ServiceLifetime.Singleton:
                AssertSingletonLifetime<TService, TImplementation>(provider, key);
                return;
            case ServiceLifetime.Scoped:
                AssertScopedLifetime<TService, TImplementation>(provider, key);
                return;
            case ServiceLifetime.Transient:
                AssertTransientLifetime<TService, TImplementation>(provider, key);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(expectedLifetime), expectedLifetime, "Unsupported lifetime.");
        }
    }

    private static void AssertSingletonLifetime<TService, TImplementation>(IServiceProvider provider, object? key)
        where TService : notnull
    {
        var first = GetRequiredService<TService>(provider, key);
        var second = GetRequiredService<TService>(provider, key);

        Assert.True(ReferenceEquals(first, second));
        Assert.IsType<TImplementation>(first);

        using var scope = provider.CreateScope();
        var scoped = GetRequiredService<TService>(scope.ServiceProvider, key);
        Assert.True(ReferenceEquals(first, scoped));
    }

    private static void AssertScopedLifetime<TService, TImplementation>(IServiceProvider provider, object? key)
        where TService : notnull
    {
        using var scope = provider.CreateScope();
        var first = GetRequiredService<TService>(scope.ServiceProvider, key);
        var second = GetRequiredService<TService>(scope.ServiceProvider, key);

        Assert.True(ReferenceEquals(first, second));
        Assert.IsType<TImplementation>(first);

        using var otherScope = provider.CreateScope();
        var third = GetRequiredService<TService>(otherScope.ServiceProvider, key);
        Assert.False(ReferenceEquals(first, third));
        Assert.IsType<TImplementation>(third);
    }

    private static void AssertTransientLifetime<TService, TImplementation>(IServiceProvider provider, object? key)
        where TService : notnull
    {
        using var scope = provider.CreateScope();
        var first = GetRequiredService<TService>(scope.ServiceProvider, key);
        var second = GetRequiredService<TService>(scope.ServiceProvider, key);

        Assert.False(ReferenceEquals(first, second));
        Assert.IsType<TImplementation>(first);
        Assert.IsType<TImplementation>(second);
    }

    private static TService GetRequiredService<TService>(IServiceProvider provider, object? key)
        where TService : notnull
    {
        return key is null
            ? provider.GetRequiredService<TService>()
            : provider.GetRequiredKeyedService<TService>(key);
    }
}
