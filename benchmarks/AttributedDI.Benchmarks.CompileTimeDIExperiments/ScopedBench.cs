using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.MEDI;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments;

[MemoryDiagnoser]
public class ScopedBench
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
    [BenchmarkCategory("Scoped")]
    public ScopedService1 Medi()
    {
        using var scope = _mediProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 DictionaryDelegates()
    {
        using var scope = _dictionaryDelegatesProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 DictionaryFunctionPointers()
    {
        using var scope = _dictionaryFunctionPointersProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 TypedDelegates()
    {
        using var scope = _typedDelegatesProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }
}
