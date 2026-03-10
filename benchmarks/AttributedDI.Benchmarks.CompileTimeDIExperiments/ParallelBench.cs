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
public class ParallelBench
{
    private const int BatchCount = 8;

    private IServiceProvider? _mediProvider;
    private IServiceProvider? _dictionaryDelegatesProvider;
    private IServiceProvider? _dictionaryDelegatesWithRuntimeTypeHandleProvider;
    private IServiceProvider? _dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider;
    private IServiceProvider? _dictionaryFunctionPointersProvider;
    private IServiceProvider? _dictionaryFunctionPointersWithRuntimeTypeHandleProvider;
    private TypedDelegatesServiceProvider? _typedDelegatesProvider;

    [Params(4, 16)]
    public int DegreeOfParallelism { get; set; }

    [Params(64, 256)]
    public int ScopesPerBatch { get; set; }

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
    [BenchmarkCategory("Parallel")]
    public Task Medi()
    {
        return ResolveTransientAcrossParallelScopes(_mediProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task DictionaryDelegates()
    {
        return ResolveTransientAcrossParallelScopes(_dictionaryDelegatesProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task DictionaryDelegatesWithRuntimeTypeHandle()
    {
        return ResolveTransientAcrossParallelScopes(_dictionaryDelegatesWithRuntimeTypeHandleProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots()
    {
        return ResolveTransientAcrossParallelScopes(_dictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task DictionaryFunctionPointers()
    {
        return ResolveTransientAcrossParallelScopes(_dictionaryFunctionPointersProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task DictionaryFunctionPointersWithRuntimeTypeHandle()
    {
        return ResolveTransientAcrossParallelScopes(_dictionaryFunctionPointersWithRuntimeTypeHandleProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task TypedDelegates()
    {
        return ResolveTransientAcrossParallelScopes((IServiceProvider)_typedDelegatesProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task TypedDelegatesDirect()
    {
        return ResolveTransientAcrossParallelScopes(_typedDelegatesProvider!);
    }

    private async Task ResolveTransientAcrossParallelScopes(IServiceProvider provider)
    {
        for (var i = 0; i < BatchCount; i++)
        {
            var workers = new Task[DegreeOfParallelism];

            for (var workerIndex = 0; workerIndex < workers.Length; workerIndex++)
            {
                var capturedWorkerIndex = workerIndex;
                workers[workerIndex] = Task.Run(() => RunWorker(provider, capturedWorkerIndex));
            }

            await Task.WhenAll(workers);
        }
    }

    private async Task ResolveTransientAcrossParallelScopes(TypedDelegatesServiceProvider provider)
    {
        for (var i = 0; i < BatchCount; i++)
        {
            var workers = new Task[DegreeOfParallelism];

            for (var workerIndex = 0; workerIndex < workers.Length; workerIndex++)
            {
                var capturedWorkerIndex = workerIndex;
                workers[workerIndex] = Task.Run(() => RunWorker(provider, capturedWorkerIndex));
            }

            await Task.WhenAll(workers);
        }
    }

    private void RunWorker(IServiceProvider provider, int workerIndex)
    {
        for (var i = 0; i < GetScopeCountForWorker(workerIndex); i++)
        {
            using var scope = provider.CreateScope();
            _ = scope.ServiceProvider.GetRequiredService<TransientService1>();
        }
    }

    private void RunWorker(TypedDelegatesServiceProvider provider, int workerIndex)
    {
        for (var i = 0; i < GetScopeCountForWorker(workerIndex); i++)
        {
            using var scope = provider.CreateScope();
            _ = scope.GetRequiredService<TransientService1>();
        }
    }

    private int GetScopeCountForWorker(int workerIndex)
    {
        var scopesPerWorker = ScopesPerBatch / DegreeOfParallelism;
        var remainder = ScopesPerBatch % DegreeOfParallelism;
        return workerIndex < remainder ? scopesPerWorker + 1 : scopesPerWorker;
    }
}
