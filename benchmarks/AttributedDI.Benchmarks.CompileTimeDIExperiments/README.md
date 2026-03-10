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
