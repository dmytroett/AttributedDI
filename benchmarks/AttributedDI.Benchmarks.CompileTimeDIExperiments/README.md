To run the benchmark:

```sh
sudo dotnet run -c Release -- --filter "*ParallelBench*" "*ScopedBench*" "*SingletonBench*" "*TransientBench*" -m -p EP -d --disasmDepth 10
```

Options:

- `-m` - optional, enables memory diagnoser. All of the benchmarks already have it enabled via attribute by default, so redundand in most of the cases.
- `-p EP` - optional, enables `EventPipeProfiler`. The benchmark will also produce a trace file. Useful to review and analyze for potential improvements.
- `-d` enables DisassemblyDiagnoser and exports diassembly of benchmarked code.
  - `--disasmDepth` - Sets the recursive depth for the disassembler.
  - `--disasmDiff` - Generates diff reports for the disassembler.

By default the benchmark is configured to aggregate results by category into a single report. So no extra flags necessary.

# Summary

The perfect shape (for now) looks like this:

- slow path with `FrozenDictionary<RuntimeTypeHandle, Func<Ctx, object>>`.
- fast path with `TypedResolver<T>.Resolve`.
- `object?[]` based root scope cache for singleton/scoped services with volatile slot based lookup and double-check-locking (this is magnitudes faster).
- Cache per provider I guess. One potential problem could be with interface implementation of the CTX. However I think dynamic PGO will be able to devirtualize the call to make sure that it is performant.

Alternative shape could be a design 4 with following improvements:

- Slow path with `FrozenDictionary<RuntimeTypeHandle, Func<Ctx, object>>`. This to be static does something like `((MySpecificProvider)ctx.Parts[ProviderId]).ResolveIClock()`. I can come up with a way to generate a cheap closure to capture ProviderId, so it should be equivalent to above.
- Similar fast path with TypedResolver. Again, with correct closure, should have the same perf.
- Per part cache option. For example based on the number of services we can do either sparse cache with Dictionary, or object?[] slot based lookup.

So this option is much easier to implement, as it is much easier to isolate parts that have to be source generated vs static. As a downside - a lot of repetitive code. Not sure it is not going to cause issues with instruction cache explosion. To make the matter worse - it is hard to benchmark this option, as it depends on so many factors, like size of dependency graph, number of providers, etc.

Looks like the benchmark is dominated by how slow singleton resolution is, considering that each service directly or indirectly depend on a singleton service. Maybe I need to check benchmarks and figure out potential perf improvements in that area. Also check profiles in general, maybe there is something interesting.
