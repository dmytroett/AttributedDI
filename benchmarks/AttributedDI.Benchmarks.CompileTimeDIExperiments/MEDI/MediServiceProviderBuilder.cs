using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.MEDI;

public static class MediServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton<SingletonService1>();
        services.AddSingleton<SingletonService2>();
        services.AddSingleton<SingletonService3>();

        services.AddScoped<ScopedService1>();
        services.AddScoped<ScopedService2>();
        services.AddScoped<ScopedService3>();

        services.AddTransient<TransientService1>();
        services.AddTransient<TransientService2>();
        services.AddTransient<TransientService3>();
        services.AddTransient<TransientService4>();

        return services.BuildServiceProvider();
    }
}
