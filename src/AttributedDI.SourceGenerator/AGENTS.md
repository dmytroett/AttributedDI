# Source Generator Instructions

Best practices for developing incremental source generators, based on official Roslyn documentation.

References:
- Incremental Generators Design: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
- Incremental Generators Cookbook: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md

## Core Principles

### Pipeline Model Design

Source generators must build value-equatable pipelines to enable incremental compilation caching.

- Use `record` types for data models (value equality is generated automatically)
- Never put `ISymbol` instances in pipeline models
- Never put `SyntaxNode` instances in models
- Never put `Location` instances in models
- Extract information from symbols/syntax as early as possible in the pipeline
- Use `ImmutableArray<T>` instead of `List<T>` or `T[]` in models

Example anti-pattern to avoid:
```csharp
// DON'T do this - ISymbol prevents garbage collection
private record ServiceInfo(INamedTypeSymbol Symbol, string Name);
```

Correct pattern:
```csharp
// DO this - extract what you need as strings
private record ServiceInfo(string Namespace, string Name, string FullyQualifiedName);
```

### Use `ForAttributeWithMetadataName`

This is much more efficient than `CreateSyntaxProvider`.

- Always use `SyntaxProvider.ForAttributeWithMetadataName` when targeting attributes
- Fully-qualified metadata names format: `Namespace.ClassName` (with backticks for generic parameters, e.g., `My.Namespace.MyAttribute`1`)
- The built-in heuristic can eliminate most syntax nodes before any user code runs
- Provides two lambdas:
  - `predicate`: Runs only on files that might contain the attribute (syntactic check)
  - `transform`: Runs on all matching nodes to capture semantic information

Example:
```csharp
var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
    fullyQualifiedMetadataName: "AttributedDI.RegisterAsAttribute",
    predicate: static (node, _) => node is ClassDeclarationSyntax,
    transform: static (ctx, ct) =>
    {
        // Extract information from symbols here, not in the predicate
        var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
        return new ServiceModel(symbol.Name, symbol.ContainingNamespace?.ToDisplayString());
    });
```

## Performance Best Practices

### Combine Order Matters

Extract information from expensive sources (like `CompilationProvider`) before combining.

Inefficient:
```csharp
var combined = texts.Combine(context.CompilationProvider);
```

Efficient:
```csharp
var assemblyName = context.CompilationProvider
    .Select(static (c, _) => c.AssemblyName);
var combined = texts.Combine(assemblyName);
```

### Minimize Collection Types in Models

- Wrap `ImmutableArray<T>` in your models with custom equality if needed
- Built-in collection types (`List<T>`, `T[]`) use reference equality
- Consider creating wrapper types for better caching

### Custom Comparers

Use `WithComparer()` when default equality isn't sufficient:

```csharp
var pipeline = context.AdditionalTextsProvider
    .Select(static (text, _) => text.Path)
    .WithComparer(StringComparer.OrdinalIgnoreCase);
```

## Incremental Caching Strategy

### Design for Caching

Break operations into small transformation steps to maximize cache hit opportunities.

- Each transformation is a checkpoint
- If a checkpoint produces the same value as before, everything downstream is skipped
- More transformations = more opportunities to cache

Example:
```csharp
var names = items.Select(static i => i.Name);
var prefixed = names.Select(static n => "prefix_" + n);
var collected = prefixed.Collect();
```

### Determinism is Required

- All transformations must be deterministic
- Same input must always produce identical output
- Avoid `Guid.NewGuid()`, `DateTime.Now`, `Random`, etc.
- Be careful with `Dictionary` iteration order (consider `ImmutableSortedDictionary`)

## Cancellation Handling

- Always forward `CancellationToken` to Roslyn APIs that accept it
- For expensive operations, call `cancellationToken.ThrowIfCancellationRequested()` at regular intervals
- Never save partially generated results to work around cancellation

```csharp
var expensive = txtFilesArray.Select(static (files, cancellationToken) =>
{
    foreach (var file in files)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // expensive operation...
    }
});
```

## Patterns to Avoid

- Scanning for indirectly implemented interfaces
- Scanning for indirectly inherited types
- Scanning for marker attributes on base types
- Non-sealed marker attributes that expect inheritance

These cause massive performance degradation in IDEs.
