using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandle;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointersWithRuntimeTypeHandle;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.MEDI;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments;

[MemoryDiagnoser]
public class SingletonBench
{
    private IServiceProvider? _mediProvider;
    private IServiceProvider? _dictionaryDelegatesProvider;
    private IServiceProvider? _dictionaryDelegatesWithRuntimeTypeHandleProvider;
    private IServiceProvider? _dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider;
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
        _dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider =
            DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProviderBuilder.BuildServiceProvider();
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
        BenchmarkDisposer.DisposeProvider(_dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryFunctionPointersProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryFunctionPointersWithRuntimeTypeHandleProvider);
        BenchmarkDisposer.DisposeProvider(_typedDelegatesProvider);

        _mediProvider = null;
        _dictionaryDelegatesProvider = null;
        _dictionaryDelegatesWithRuntimeTypeHandleProvider = null;
        _dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider = null;
        _dictionaryFunctionPointersProvider = null;
        _dictionaryFunctionPointersWithRuntimeTypeHandleProvider = null;
        _typedDelegatesProvider = null;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Singleton")]
    public SingletonService3 Medi()
    {
        return _mediProvider!.GetRequiredService<SingletonService3>();
    }

    [Benchmark]
    [BenchmarkCategory("Singleton")]
    public SingletonService3 DictionaryDelegates()
    {
        return _dictionaryDelegatesProvider!.GetRequiredService<SingletonService3>();
    }

    [Benchmark]
    [BenchmarkCategory("Singleton")]
    public SingletonService3 DictionaryDelegatesWithRuntimeTypeHandle()
    {
        return _dictionaryDelegatesWithRuntimeTypeHandleProvider!.GetRequiredService<SingletonService3>();
    }

    [Benchmark]
    [BenchmarkCategory("Singleton")]
    public SingletonService3 DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots()
    {
        return _dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider!
            .GetRequiredService<SingletonService3>();
    }

    [Benchmark]
    [BenchmarkCategory("Singleton")]
    public SingletonService3 DictionaryFunctionPointers()
    {
        return _dictionaryFunctionPointersProvider!.GetRequiredService<SingletonService3>();
    }

    [Benchmark]
    [BenchmarkCategory("Singleton")]
    public SingletonService3 DictionaryFunctionPointersWithRuntimeTypeHandle()
    {
        return _dictionaryFunctionPointersWithRuntimeTypeHandleProvider!.GetRequiredService<SingletonService3>();
    }

    [Benchmark]
    [BenchmarkCategory("Singleton")]
    public SingletonService3 TypedDelegates()
    {
        return ((IServiceProvider)_typedDelegatesProvider!).GetRequiredService<SingletonService3>();
    }

    [Benchmark]
    [BenchmarkCategory("Singleton")]
    public SingletonService3 TypedDelegatesDirect()
    {
        return _typedDelegatesProvider!.GetRequiredService<SingletonService3>();
    }
}
