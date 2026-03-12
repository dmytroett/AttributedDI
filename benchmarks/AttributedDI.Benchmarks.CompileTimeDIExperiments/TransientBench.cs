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
public class TransientBench
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
    [BenchmarkCategory("Transient")]
    public TransientService1 Medi()
    {
        return ResolveTransientTwice(_mediProvider!);
    }

    /*
    [Benchmark]
    [BenchmarkCategory("Transient")]
    public TransientService1 DictionaryDelegates()
    {
        return ResolveTransientTwice(_dictionaryDelegatesProvider!);
    }
    */

    [Benchmark]
    [BenchmarkCategory("Transient")]
    public TransientService1 DictionaryDelegatesWithRuntimeTypeHandle()
    {
        return ResolveTransientTwice(_dictionaryDelegatesWithRuntimeTypeHandleProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Transient")]
    public TransientService1 DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots()
    {
        return ResolveTransientTwice(_dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Transient")]
    public TransientService1 DictionaryDelegatesWithRuntimeTypeHandleAndSlots()
    {
        return ResolveTransientTwice(_dictionaryDelegatesWithRuntimeTypeHandleAndSlotsProvider!);
    }

    /*
    [Benchmark]
    [BenchmarkCategory("Transient")]
    public TransientService1 DictionaryFunctionPointers()
    {
        return ResolveTransientTwice(_dictionaryFunctionPointersProvider!);
    }
    */

    [Benchmark]
    [BenchmarkCategory("Transient")]
    public TransientService1 DictionaryFunctionPointersWithRuntimeTypeHandle()
    {
        return ResolveTransientTwice(_dictionaryFunctionPointersWithRuntimeTypeHandleProvider!);
    }

    // [Benchmark]
    // [BenchmarkCategory("Transient")]
    // public TransientService1 TypedDelegates()
    // {
    //     return ResolveTransientTwice((IServiceProvider)_typedDelegatesProvider!);
    // }

    // [Benchmark]
    // [BenchmarkCategory("Transient")]
    // public TransientService1 TypedDelegatesDirect()
    // {
    //     return ResolveTransientTwice(_typedDelegatesProvider!);
    // }

    [Benchmark]
    [BenchmarkCategory("Transient")]
    public TransientService1 TypedDelegatesWithRuntimeTypeHandleAndSlots()
    {
        return ResolveTransientTwice((IServiceProvider)_typedDelegatesWithRuntimeTypeHandleAndSlotsProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Transient")]
    public TransientService1 TypedDelegatesWithRuntimeTypeHandleAndSlotsDirect()
    {
        return ResolveTransientTwice(_typedDelegatesWithRuntimeTypeHandleAndSlotsProvider!);
    }

    private static TransientService1 ResolveTransientTwice(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        _ = scope.ServiceProvider.GetRequiredService<TransientService1>();
        return scope.ServiceProvider.GetRequiredService<TransientService1>();
    }

    private static TransientService1 ResolveTransientTwice(TypedDelegatesServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        _ = scope.GetRequiredService<TransientService1>();
        return scope.GetRequiredService<TransientService1>();
    }

    private static TransientService1 ResolveTransientTwice(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        _ = scope.GetService<TransientService1>();
        return scope.GetService<TransientService1>()!;
    }
}
