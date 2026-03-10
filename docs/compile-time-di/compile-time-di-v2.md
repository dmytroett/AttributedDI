# Compile-Time DI V2 Ideas

This document captures V2-only directions that are intentionally out of V1 scope.

## Public API direction

- Keep V1 integration points unless benchmarks justify a breaking change.
- Prefer opt-in switches for advanced behaviors.
- Avoid API changes that diverge too far from MEDI unless there is a clear payoff.

Potential opt-in settings:

```xml
<PropertyGroup>
  <GenerateCompileTimeServiceProvider>true</GenerateCompileTimeServiceProvider>
  <CompileTimeDiDisableInternalTypeResolution>false</CompileTimeDiDisableInternalTypeResolution>
  <CompileTimeDiUseLockFreeScopedCache>false</CompileTimeDiUseLockFreeScopedCache>
</PropertyGroup>
```

Property names are placeholders.

## Implementation ideas

### Heuristic strategy selection

Pick a generated resolution strategy by service count:

- `count < 10`: direct `if`/`else`.
- `10 <= count < 100`: slot dispatch.
- `100 <= count < 1000`: chunked slot dispatch.
- `count >= 1000`: dictionary-based dispatch.

### Scoped cache shape heuristics

- Use `object[]` when scoped slot count is small and dense.
- Use dictionary storage when slot count is sparse or unknown.

### Dropping dictionary lookups (experimental)

A possible optimization is replacing `FrozenDictionary` with array indexing if slot IDs can become a global perfect hash in the entry project.

Constraints:

- Internal type resolution across assemblies becomes harder.
- Exporting enough metadata may require many generated attributes.
- Aggregator complexity increases, especially with keyed services and versioning.

This remains experimental until benchmarks prove a clear benefit.

## MEDI comparison

TODO

