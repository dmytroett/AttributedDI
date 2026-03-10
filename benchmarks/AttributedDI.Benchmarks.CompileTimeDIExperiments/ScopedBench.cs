using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandle;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointersWithRuntimeTypeHandle;
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
    private IServiceProvider? _dictionaryDelegatesWithRuntimeTypeHandleProvider;
    private IServiceProvider? _dictionaryFunctionPointersProvider;
    private IServiceProvider? _dictionaryFunctionPointersWithRuntimeTypeHandleProvider;
    private TypedDelegatesServiceProvider? _typedDelegatesProvider;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mediProvider = MediServiceProviderBuilder.BuildServiceProvider();
        _dictionaryDelegatesProvider = DictionaryDelegatesServiceProviderBuilder.BuildServiceProvider();
        _dictionaryDelegatesWithRuntimeTypeHandleProvider =
            DictionaryDelegatesWithRuntimeTypeHandleServiceProviderBuilder.BuildServiceProvider();
        _dictionaryFunctionPointersProvider = DictionaryFunctionPointersServiceProviderBuilder.BuildServiceProvider();
        _dictionaryFunctionPointersWithRuntimeTypeHandleProvider =
            DictionaryFunctionPointersWithRuntimeTypeHandleServiceProviderBuilder.BuildServiceProvider();
        _typedDelegatesProvider = TypedDelegatesServiceProviderBuilder.BuildTypedServiceProvider();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        BenchmarkDisposer.DisposeProvider(_mediProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryDelegatesProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryDelegatesWithRuntimeTypeHandleProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryFunctionPointersProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryFunctionPointersWithRuntimeTypeHandleProvider);
        BenchmarkDisposer.DisposeProvider(_typedDelegatesProvider);

        _mediProvider = null;
        _dictionaryDelegatesProvider = null;
        _dictionaryDelegatesWithRuntimeTypeHandleProvider = null;
        _dictionaryFunctionPointersProvider = null;
        _dictionaryFunctionPointersWithRuntimeTypeHandleProvider = null;
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
    public ScopedService1 DictionaryDelegatesWithRuntimeTypeHandle()
    {
        using var scope = _dictionaryDelegatesWithRuntimeTypeHandleProvider!.CreateScope();
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
    public ScopedService1 DictionaryFunctionPointersWithRuntimeTypeHandle()
    {
        using var scope = _dictionaryFunctionPointersWithRuntimeTypeHandleProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 TypedDelegates()
    {
        using var scope = _typedDelegatesProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 TypedDelegatesDirect()
    {
        using var scope = _typedDelegatesProvider!.CreateScope();
        return scope.GetRequiredService<ScopedService1>();
    }
}
