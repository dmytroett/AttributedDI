using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;
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
    private IServiceProvider? _dictionaryFunctionPointersProvider;
    private IServiceProvider? _typedDelegatesProvider;
    private ParallelOptions? _parallelOptions;

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
        _parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = DegreeOfParallelism };
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
        _parallelOptions = null;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Parallel")]
    public Task Medi()
    {
        return ResolveTransientAcrossParallelScopes(_mediProvider!, GetParallelOptions());
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task DictionaryDelegates()
    {
        return ResolveTransientAcrossParallelScopes(_dictionaryDelegatesProvider!, GetParallelOptions());
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task DictionaryFunctionPointers()
    {
        return ResolveTransientAcrossParallelScopes(_dictionaryFunctionPointersProvider!, GetParallelOptions());
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public Task TypedDelegates()
    {
        return ResolveTransientAcrossParallelScopes(_typedDelegatesProvider!, GetParallelOptions());
    }

    private async Task ResolveTransientAcrossParallelScopes(IServiceProvider provider, ParallelOptions parallelOptions)
    {
        for (var i = 0; i < BatchCount; i++)
        {
            await Parallel.ForAsync(0, ScopesPerBatch, parallelOptions, async (_, _) =>
            {
                await using var scope = provider.CreateAsyncScope();
                _ = scope.ServiceProvider.GetRequiredService<TransientService1>();
            });
        }
    }
    private ParallelOptions GetParallelOptions()
    {
        return _parallelOptions ?? throw new InvalidOperationException("Parallel options are not initialized.");
    }
}
