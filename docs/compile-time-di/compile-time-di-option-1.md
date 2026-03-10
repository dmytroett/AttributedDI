# Compile-Time DI Option 1: Dictionary-Based Factory Resolution

This option exports service factories from each provider part, then aggregates them into frozen dictionaries in the root provider.

## Public API surface

### Registration model

```csharp
public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

internal sealed class Session(ILogger logger) : IDisposable, IAsyncDisposable
{
    // Dispose / DisposeAsync omitted for brevity.
}

public sealed class Repository(Session session);

[CompileTimeServiceProviderPart]
[Register(Lifetime.Singleton, typeof(SystemClock), typeof(IClock))]
[Register(Lifetime.Scoped, typeof(Session))]
[Register(Lifetime.Transient, typeof(Repository))]
public partial class MyServiceProviderPart;
```

### Runtime contracts (static package)

```csharp
public interface IScopeCache : IDisposable, IAsyncDisposable
{
    T GetOrCreate<T>(
        Type serviceType,
        CompileTimeProviderContext context,
        Func<CompileTimeProviderContext, T> factory);

    void CaptureDisposable(object instance);
}

public readonly record struct CompileTimeProviderContext(
    IScopeCache Root,
    IScopeCache Current,
    IServiceProvider ExternalDependencyResolver);
```

`ExternalDependencyResolver` points to the composed provider for the current root/scope so one part can resolve dependencies from other parts before falling back to runtime registrations.

```csharp
// disposal and thread-safety details are intentionally trimmed.
internal sealed class ArrayBasedScopeCache(int[] perProviderCapacities) : IScopeCache
{
    private readonly object _lock = new();
    private readonly object?[][] _slots = new object?[perProviderCapacities.Length][];
    private readonly List<object> _disposables = [];
}

internal sealed class DictionaryBasedScopeCache(int capacity) : IScopeCache
{
    private readonly object _lock = new();
    private readonly Dictionary<Type, object> _cache = new(capacity);
    private readonly List<object> _disposables = [];
}
```

## Implementation details

### Generated part shape

```csharp
public interface ICompiledServiceProviderPart
{
    static abstract IReadOnlyCollection<(
        Type ServiceType,
        Func<CompileTimeProviderContext, object> Factory)> ExportedServices { get; }

    static abstract IReadOnlyCollection<(
        Type ServiceType,
        object? Key,
        Func<CompileTimeProviderContext, object> Factory)> ExportedKeyedServices { get; }
}

public sealed class MyServiceProviderPart : ICompiledServiceProviderPart
{
    public static IReadOnlyCollection<(Type, Func<CompileTimeProviderContext, object>)> ExportedServices => ...;
    public static IReadOnlyCollection<(Type, object?, Func<CompileTimeProviderContext, object>)> ExportedKeyedServices => ...;
}

[assembly: ExportsProviderPart(typeof(MyServiceProviderPart), singletonCount: 1, scopedCount: 1, transientCount: 1)]
```

### Root provider and factory

This part is static, so we don't need to source-generate it.

```csharp
public sealed record AttributedDiContainerBuilder(IServiceCollection Services)
{
    public List<Type> ProviderPartTypes { get; } = [];
}

public sealed class AttributedDiServiceProviderFactory
    : IServiceProviderFactory<AttributedDiContainerBuilder>
{
    public AttributedDiContainerBuilder CreateBuilder(IServiceCollection services) => new(services);

    public IServiceProvider CreateServiceProvider(AttributedDiContainerBuilder builder)
    {
        var fallback = builder.Services.BuildServiceProvider();
        var resolvers = BuildResolvers(builder.ProviderPartTypes);       // Type -> factory
        var keyedResolvers = BuildKeyedResolvers(builder.ProviderPartTypes); // (Type,key) -> factory
        return new AttributedDiRootServiceProvider(resolvers, keyedResolvers, fallback);
    }
}
```

```csharp
internal sealed class AttributedDiRootServiceProvider : IServiceProvider, IServiceScopeFactory
{
    private readonly FrozenDictionary<Type, Func<CompileTimeProviderContext, object>> _resolvers;
    private readonly FrozenDictionary<(Type, object?), Func<CompileTimeProviderContext, object>> _keyedResolvers;
    private readonly IServiceProvider _fallback;
    private readonly CompileTimeProviderContext _context;

    public AttributedDiRootServiceProvider(
        FrozenDictionary<Type, Func<CompileTimeProviderContext, object>> resolvers,
        FrozenDictionary<(Type, object?), Func<CompileTimeProviderContext, object>> keyedResolvers,
        IServiceProvider fallback)
    {
        _resolvers = resolvers;
        _keyedResolvers = keyedResolvers;
        _fallback = fallback;
        _context = new(new ArrayBasedScopeCache(/* generated sizes */ []), new ArrayBasedScopeCache([]), this);
    }

    public object? GetService(Type serviceType)
    {
        if (_resolvers.TryGetValue(serviceType, out var factory))
        {
            return factory(_context);
        }

        return _fallback.GetService(serviceType);
    }

    public IServiceScope CreateScope() => new AttributedDiServiceScope(this, _fallback.CreateScope());

    // Keyed and disposal members omitted for brevity.
}
```

## Source-generation opt-in (entry project)

```xml
<PropertyGroup>
  <GenerateCompileTimeServiceProvider>true</GenerateCompileTimeServiceProvider>
</PropertyGroup>
```

When enabled, the entry project can emit:

```csharp
public static class AttributedDiContainerBuilderExtensions
{
    public static AttributedDiContainerBuilder RegisterGeneratedProviders(
        this AttributedDiContainerBuilder builder)
    {
        // Generated list of discovered part types.
        builder.ProviderPartTypes.Add(typeof(MyServiceProviderPart));
        return builder;
    }
}
```

## Notes

- This is the simplest model and likely the easiest to stabilize first.
- We can potentially use function pointer variant like this:

```csharp
public interface ICompiledServiceProviderPart
{
    static abstract IReadOnlyCollection<(Type ServiceType, IntPtr FactoryPointer)> ExportedServices { get; }

    static abstract IReadOnlyCollection<(Type ServiceType, object? Key, IntPtr FactoryPointer)> ExportedKeyedServices { get; }
}

internal sealed class AttributedDiRootServiceProvider
{
    private readonly FrozenDictionary<Type, delegate*<CompileTimeProviderContext, object>> _resolvers;
    private readonly FrozenDictionary<(Type, object?), delegate*<CompileTimeProviderContext, object>> _keyedResolvers;
}
```

This works, because the source generated implementation of `ICompiledServiceProviderPart` can use `Method.MethodHandle.GetFunctionPointer()` in safe context, while `IServiceProvider` implementation can be compiled with `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`.

- `FrozenDictionary<Type, ...>` should be benchmarked before considering `RuntimeTypeHandle` alternatives.

## MEDI comparison

TODO
