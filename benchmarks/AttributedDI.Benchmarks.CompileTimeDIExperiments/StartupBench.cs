using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandle;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointersWithRuntimeTypeHandle;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.MEDI;
using AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;
using BenchmarkDotNet.Attributes;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments;

// temporarily comment out, as I care less about startup.
// [MemoryDiagnoser]
// public class StartupBench
// {
//     [Benchmark(Baseline = true)]
//     [BenchmarkCategory("Startup")]
//     public void Medi()
//     {
//         var provider = MediServiceProviderBuilder.BuildServiceProvider();
//         BenchmarkDisposer.DisposeProvider(provider);
//     }

//     [Benchmark]
//     [BenchmarkCategory("Startup")]
//     public void DictionaryDelegates()
//     {
//         var provider = DictionaryDelegatesServiceProviderBuilder.BuildServiceProvider();
//         BenchmarkDisposer.DisposeProvider(provider);
//     }

//     [Benchmark]
//     [BenchmarkCategory("Startup")]
//     public void DictionaryDelegatesWithRuntimeTypeHandle()
//     {
//         var provider = DictionaryDelegatesWithRuntimeTypeHandleServiceProviderBuilder.BuildServiceProvider();
//         BenchmarkDisposer.DisposeProvider(provider);
//     }

//     [Benchmark]
//     [BenchmarkCategory("Startup")]
//     public void DictionaryFunctionPointers()
//     {
//         var provider = DictionaryFunctionPointersServiceProviderBuilder.BuildServiceProvider();
//         BenchmarkDisposer.DisposeProvider(provider);
//     }

//     [Benchmark]
//     [BenchmarkCategory("Startup")]
//     public void DictionaryFunctionPointersWithRuntimeTypeHandle()
//     {
//         var provider = DictionaryFunctionPointersWithRuntimeTypeHandleServiceProviderBuilder.BuildServiceProvider();
//         BenchmarkDisposer.DisposeProvider(provider);
//     }

//     [Benchmark]
//     [BenchmarkCategory("Startup")]
//     public void TypedDelegates()
//     {
//         var provider = TypedDelegatesServiceProviderBuilder.BuildServiceProvider();
//         BenchmarkDisposer.DisposeProvider(provider);
//     }
// }
