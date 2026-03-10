# Compile-Time DI Option 3: Typed Fast Path with Dictionary Fallback

This option keeps option 1 semantics but adds a generic fast path for typed resolution.

## Public API surface

### Registration model

Registration input can stay identical to option 1 or option 2. The difference is in generated resolver wiring, not registration syntax.

### Runtime contracts (typed path)

```csharp
public readonly record struct CompileTimeProviderContext(
    IScopeCache Root,
    IScopeCache Current,
    IServiceProvider FallbackProvider);

public interface ICompiledServiceProvider
{
    T? GetService<T>(CompileTimeProviderContext context);
    T? GetKeyedService<T>(object? key, CompileTimeProviderContext context);
}

public static class CompiledResolver<T>
{
    public static Func<ICompiledServiceProvider, CompileTimeProviderContext, T>? Resolve;
}
```

## Implementation details

### Generated part shape

```csharp
public interface ICompiledServiceProviderPart
{
    static abstract IReadOnlyCollection<(
        Type ServiceType,
        Func<ICompiledServiceProvider, CompileTimeProviderContext, object> Factory)> ExportedServices { get; }

    static abstract IReadOnlyCollection<(
        Type ServiceType,
        object? Key,
        Func<ICompiledServiceProvider, CompileTimeProviderContext, object> Factory)> ExportedKeyedServices { get; }

    // Generated assignment into CompiledResolver<T>.Resolve
    static abstract void InitializeTypedResolvers();
}
```

### Provider implementation

```csharp
public sealed class AttributedDiCompiledServiceProvider : ICompiledServiceProvider, IServiceProvider
{
    private readonly FrozenDictionary<Type, Func<ICompiledServiceProvider, CompileTimeProviderContext, object>> _untypedResolvers;
    private readonly IServiceProvider _fallback;
    private readonly CompileTimeProviderContext _context;

    public T? GetService<T>(CompileTimeProviderContext context)
    {
        var typedFactory = CompiledResolver<T>.Resolve;
        if (typedFactory is not null)
        {
            return typedFactory(this, context);
        }

        return context.FallbackProvider.GetService<T>();
    }

    public object? GetService(Type serviceType)
    {
        if (_untypedResolvers.TryGetValue(serviceType, out var factory))
        {
            return factory(this, _context);
        }

        return _fallback.GetService(serviceType);
    }

    public T? GetKeyedService<T>(object? key, CompileTimeProviderContext context)
    {
        // Keyed path omitted for brevity.
        return default;
    }
}
```

## Notes

- Untyped `GetService(Type)` still needs dictionary lookup for compatibility.
- An interceptor can be implemented to replace call sites that use `GetService<T>()` and `GetRequiredService<T>()` to use the typed variants.
- A function-pointer variant (`delegate*`) may reduce delegate overhead but increases complexity and unsafe usage.

## MEDI comparison

TODO
