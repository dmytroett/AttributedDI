using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.MEDI;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments;

[MemoryDiagnoser]
public class TransientBench
{
    private IServiceProvider? _mediProvider;
    private IServiceProvider? _dictionaryDelegatesProvider;
    private IServiceProvider? _dictionaryFunctionPointersProvider;
    private IServiceProvider? _typedDelegatesProvider;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mediProvider = MediServiceProviderBuilder.BuildServiceProvider();
        _dictionaryDelegatesProvider = DictionaryDelegatesServiceProviderBuilder.BuildServiceProvider();
        _dictionaryFunctionPointersProvider = DictionaryFunctionPointersServiceProviderBuilder.BuildServiceProvider();
        _typedDelegatesProvider = TypedDelegatesServiceProviderBuilder.BuildServiceProvider();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        BenchmarkDisposer.DisposeProvider(_mediProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryDelegatesProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryFunctionPointersProvider);
        BenchmarkDisposer.DisposeProvider(_typedDelegatesProvider);

        _mediProvider = null;
        _dictionaryDelegatesProvider = null;
        _dictionaryFunctionPointersProvider = null;
        _typedDelegatesProvider = null;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Transient")]
    public Task<TransientService1> Medi()
    {
        return ResolveTransientTwice(_mediProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Transient")]
    public Task<TransientService1> DictionaryDelegates()
    {
        return ResolveTransientTwice(_dictionaryDelegatesProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Transient")]
    public Task<TransientService1> DictionaryFunctionPointers()
    {
        return ResolveTransientTwice(_dictionaryFunctionPointersProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Transient")]
    public Task<TransientService1> TypedDelegates()
    {
        return ResolveTransientTwice(_typedDelegatesProvider!);
    }

    private static async Task<TransientService1> ResolveTransientTwice(IServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        _ = scope.ServiceProvider.GetRequiredService<TransientService1>();
        return scope.ServiceProvider.GetRequiredService<TransientService1>();
    }
}
