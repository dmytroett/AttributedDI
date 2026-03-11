# Compile-Time DI Experiments

This project is a benchmark playground for service-provider shapes that may later influence AttributedDI runtime code generation. The focus here is narrow: compare lookup keys, factory dispatch strategies, and cache layouts against the default `Microsoft.Extensions.DependencyInjection` container.

## Benchmark coverage

- `StartupBench`: build and dispose a provider.
- `SingletonBench`: resolve `SingletonService3` from the root provider.
- `ScopedBench`: create a scope and resolve `ScopedService1`.
- `TransientBench`: create a scope and resolve `TransientService1` twice.
- `ParallelBench`: resolve transients across many short-lived scopes in parallel.

## Provider variants

- `Medi`: baseline `Microsoft.Extensions.DependencyInjection` provider.
- `DictionaryDelegates`: `FrozenDictionary<Type, Func<...>>` plus dictionary-based singleton and scoped caches.
- `DictionaryDelegatesWithRuntimeTypeHandle`: same delegate-based shape, but keyed by `RuntimeTypeHandle`.
- `DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots`: `RuntimeTypeHandle` lookup plus slot-based arrays for singleton and root-scoped caches.
- `DictionaryFunctionPointers`: replaces delegate factories with function pointers while keeping `Type` lookup.
- `DictionaryFunctionPointersWithRuntimeTypeHandle`: combines function pointers with `RuntimeTypeHandle` lookup.
- `TypedDelegates`: keeps a general provider surface, but adds a generated typed fast path for `GetRequiredService<T>()`.
- `TypedDelegatesDirect`: measures the same typed provider through its direct API to isolate `IServiceProvider` overhead.

## Running the benchmarks

Run everything:

```sh
dotnet run -c Release --project benchmarks/AttributedDI.Benchmarks.CompileTimeDIExperiments -- --filter "*StartupBench*" "*SingletonBench*" "*ScopedBench*" "*TransientBench*" "*ParallelBench*"
```

Useful BenchmarkDotNet options:

- `-m`: enables the memory diagnoser. This is usually redundant because the benchmark classes already use `[MemoryDiagnoser]`.
- `-p EP`: enables the `EventPipeProfiler` and produces a trace file for profiling.
- `-d`: enables the disassembly diagnoser.
- `--disasmDepth <n>`: controls recursive disassembly depth.
- `--disasmDiff`: emits disassembly diffs.

The benchmark config already groups results by category and parameter set, so no extra grouping flags are needed.

## Current design direction

The most promising shape from these experiments is still:

- `FrozenDictionary<RuntimeTypeHandle, Func<TContext, object>>` for the general resolution path.
- A generated typed fast path, similar to `TypedDelegatesCompiledResolver<T>`, for calls known at compile time.
- `object?[]` slot caches for singleton and root-scoped services, using volatile reads and double-check locking.

The remaining unresolved part is scoped caching. The current `DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots` experiment keeps per-scope instances in a dictionary, which is simple but still shows up on the hot path. The next layout worth validating is a provider-partitioned `object?[][]`, where the first dimension represents the provider part and the second dimension represents the slot.

## Alternative design direction

Another viable direction is a provider composed from generated "parts". In that model, each part owns the logic for resolving its services and maintaining the right cache shape for those services.

This could still reuse the same low-level techniques that currently look promising:

- `FrozenDictionary<RuntimeTypeHandle, Func<TContext, object>>` or a comparable generated dispatch table for the slow path.
- Generated typed fast paths for compile-time-known resolutions.
- Slot-based arrays, dictionaries, or mixed cache layouts chosen per part instead of once for the whole provider.

The main advantage is locality of responsibility. Each generated part can tailor its cache layout to the project and to the services it owns, which may be a better fit than forcing one global cache strategy across the entire provider graph.

The main downside is code duplication. If every part carries its own cache and resolution machinery, the provider loses shared cache implementations, which can increase generated code size and may create instruction-cache pressure. That tradeoff is not obviously bad enough to dismiss yet, so it is worth preserving as an explicit design branch in these notes.

## Tradeoffs still worth measuring

- Sparse scoped graphs may waste memory with slot arrays, while dictionaries pay a much higher lookup cost.
- Chunked arrays could reduce wasted space, but they make lookups slower and risk losing the memory benefit if resolutions span multiple chunks.
- A more adaptive cache shape may help for very large graphs, but the added complexity needs to justify itself with clear wins in the benchmarks.
- Part-based providers may improve cache locality and project-specific tuning, but they need measurement against code size growth and instruction-cache behavior.
