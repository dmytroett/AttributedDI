using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandle;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandleAndSlots;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointersWithRuntimeTypeHandle;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.MEDI;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegatesWithRuntimeTypeHandleAndSlots;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments;

[MemoryDiagnoser]
public class ScopedBench
{
    private IServiceProvider? _mediProvider;
    private IServiceProvider? _dictionaryDelegatesProvider;
    private IServiceProvider? _dictionaryDelegatesWithRuntimeTypeHandleProvider;
    private IServiceProvider? _dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider;
    private IServiceProvider? _dictionaryDelegatesWithRuntimeTypeHandleAndSlotsProvider;
    private IServiceProvider? _dictionaryFunctionPointersProvider;
    private IServiceProvider? _dictionaryFunctionPointersWithRuntimeTypeHandleProvider;
    private TypedDelegatesServiceProvider? _typedDelegatesProvider;
    private TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider?
        _typedDelegatesWithRuntimeTypeHandleAndSlotsProvider;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mediProvider = MediServiceProviderBuilder.BuildServiceProvider();
        _dictionaryDelegatesProvider = DictionaryDelegatesServiceProviderBuilder.BuildServiceProvider();
        _dictionaryDelegatesWithRuntimeTypeHandleProvider =
            DictionaryDelegatesWithRuntimeTypeHandleServiceProviderBuilder.BuildServiceProvider();
        _dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider =
            DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProviderBuilder.BuildServiceProvider();
        _dictionaryDelegatesWithRuntimeTypeHandleAndSlotsProvider =
            DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceProviderBuilder.BuildServiceProvider();
        _dictionaryFunctionPointersProvider = DictionaryFunctionPointersServiceProviderBuilder.BuildServiceProvider();
        _dictionaryFunctionPointersWithRuntimeTypeHandleProvider =
            DictionaryFunctionPointersWithRuntimeTypeHandleServiceProviderBuilder.BuildServiceProvider();
        _typedDelegatesProvider = TypedDelegatesServiceProviderBuilder.BuildTypedServiceProvider();
        _typedDelegatesWithRuntimeTypeHandleAndSlotsProvider =
            TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProviderBuilder.BuildTypedServiceProvider();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        BenchmarkDisposer.DisposeProvider(_mediProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryDelegatesProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryDelegatesWithRuntimeTypeHandleProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryDelegatesWithRuntimeTypeHandleAndSlotsProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryFunctionPointersProvider);
        BenchmarkDisposer.DisposeProvider(_dictionaryFunctionPointersWithRuntimeTypeHandleProvider);
        BenchmarkDisposer.DisposeProvider(_typedDelegatesProvider);
        BenchmarkDisposer.DisposeProvider(_typedDelegatesWithRuntimeTypeHandleAndSlotsProvider);

        _mediProvider = null;
        _dictionaryDelegatesProvider = null;
        _dictionaryDelegatesWithRuntimeTypeHandleProvider = null;
        _dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider = null;
        _dictionaryDelegatesWithRuntimeTypeHandleAndSlotsProvider = null;
        _dictionaryFunctionPointersProvider = null;
        _dictionaryFunctionPointersWithRuntimeTypeHandleProvider = null;
        _typedDelegatesProvider = null;
        _typedDelegatesWithRuntimeTypeHandleAndSlotsProvider = null;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 Medi()
    {
        using var scope = _mediProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    /*
    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 DictionaryDelegates()
    {
        using var scope = _dictionaryDelegatesProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }
    */

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 DictionaryDelegatesWithRuntimeTypeHandle()
    {
        using var scope = _dictionaryDelegatesWithRuntimeTypeHandleProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots()
    {
        using var scope = _dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 DictionaryDelegatesWithRuntimeTypeHandleAndSlots()
    {
        using var scope = _dictionaryDelegatesWithRuntimeTypeHandleAndSlotsProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    /*
    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 DictionaryFunctionPointers()
    {
        using var scope = _dictionaryFunctionPointersProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }
    */

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

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 TypedDelegatesWithRuntimeTypeHandleAndSlots()
    {
        using var scope = _typedDelegatesWithRuntimeTypeHandleAndSlotsProvider!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ScopedService1>();
    }

    [Benchmark]
    [BenchmarkCategory("Scoped")]
    public ScopedService1 TypedDelegatesWithRuntimeTypeHandleAndSlotsDirect()
    {
        using var scope = _typedDelegatesWithRuntimeTypeHandleAndSlotsProvider!.CreateScope();
        return scope.GetService<ScopedService1>()!;
    }
}
