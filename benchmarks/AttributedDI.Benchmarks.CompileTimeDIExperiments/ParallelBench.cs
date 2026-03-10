using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.MEDI;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments;

[MemoryDiagnoser]
public class ParallelBench
{
    private const int BatchCount = 8;

    private IServiceProvider? _mediProvider;
    private IServiceProvider? _dictionaryDelegatesProvider;
    private IServiceProvider? _dictionaryFunctionPointersProvider;
    private IServiceProvider? _typedDelegatesProvider;

    [Params(4, 16)]
    public int DegreeOfParallelism { get; set; }

    [Params(64, 256)]
    public int ScopesPerBatch { get; set; }

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
    public Task DictionaryFunctionPointers()
    {
        return ResolveTransientAcrossParallelScopes(_dictionaryFunctionPointersProvider!);
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task TypedDelegates()
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

    private void RunWorker(IServiceProvider provider, int workerIndex)
    {
        for (var i = 0; i < GetScopeCountForWorker(workerIndex); i++)
        {
            using var scope = provider.CreateScope();
            _ = scope.ServiceProvider.GetRequiredService<TransientService1>();
        }
    }

    private int GetScopeCountForWorker(int workerIndex)
    {
        var scopesPerWorker = ScopesPerBatch / DegreeOfParallelism;
        var remainder = ScopesPerBatch % DegreeOfParallelism;
        return workerIndex < remainder ? scopesPerWorker + 1 : scopesPerWorker;
    }
}
