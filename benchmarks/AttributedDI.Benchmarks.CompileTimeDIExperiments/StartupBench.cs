using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.MEDI;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;
using BenchmarkDotNet.Attributes;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments;

[MemoryDiagnoser]
public class StartupBench
{
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Startup")]
    public void Medi()
    {
        var provider = MediServiceProviderBuilder.BuildServiceProvider();
        BenchmarkDisposer.DisposeProvider(provider);
    }

    [Benchmark]
    [BenchmarkCategory("Startup")]
    public void DictionaryDelegates()
    {
        var provider = DictionaryDelegatesServiceProviderBuilder.BuildServiceProvider();
        BenchmarkDisposer.DisposeProvider(provider);
    }

    [Benchmark]
    [BenchmarkCategory("Startup")]
    public void DictionaryFunctionPointers()
    {
        var provider = DictionaryFunctionPointersServiceProviderBuilder.BuildServiceProvider();
        BenchmarkDisposer.DisposeProvider(provider);
    }

    [Benchmark]
    [BenchmarkCategory("Startup")]
    public void TypedDelegates()
    {
        var provider = TypedDelegatesServiceProviderBuilder.BuildServiceProvider();
        BenchmarkDisposer.DisposeProvider(provider);
    }
}
