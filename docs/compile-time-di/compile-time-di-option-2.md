# Compile-Time DI Option 2: Slot-Based Service Resolution

This option assigns each registration a numeric slot and resolves through generated dispatch methods.

## Public API surface

### Registration model

This option is compatible with centralized or per-type registration attributes. The runtime shape is the same either way: generated slot metadata plus resolver methods per provider part.

### Runtime contracts (static package)

```csharp
public enum Lifetime
{
    Transient = 0,
    Scoped = 1,
    Singleton = 2
}

public readonly record struct ProviderSlot(int ProviderId, int SlotId);
public readonly record struct ResolverToken(Lifetime Lifetime, int ProviderId, int SlotId);

public interface IScopeCache : IDisposable, IAsyncDisposable
{
    T GetOrCreate<T>(
        ProviderSlot slot,
        CompileTimeProviderContext context,
        Func<CompileTimeProviderContext, T> factory);

    void CaptureDisposable(object instance);
}

public readonly record struct CompileTimeProviderContext(
    IScopeCache Root,
    IScopeCache Current,
    IServiceProvider ExternalDependencyResolver);
```

```csharp
internal sealed class DictionaryBasedScopeCache(int capacity) : IScopeCache
{
    private readonly object _lock = new();
    private readonly Dictionary<ProviderSlot, object> _cache = new(capacity);
    private readonly List<object> _disposables = [];
}
```

## Implementation details

### Generated provider part

```csharp
public static class MyServiceProviderPart
{
    // Metadata lists are abbreviated.
    public static IReadOnlyList<(Type ServiceType, int Slot)> ExportedSingletonServices => ...;
    public static IReadOnlyList<(Type ServiceType, int Slot)> ExportedScopedServices => ...;
    public static IReadOnlyList<(Type ServiceType, int Slot)> ExportedTransientServices => ...;

    public static object ResolveSingleton(CompileTimeProviderContext ctx, ProviderSlot slot) =>
        slot.SlotId switch
        {
            0 => ResolveIClock(ctx, slot.ProviderId),
            _ => throw new NotSupportedException()
        };

    public static object ResolveScoped(CompileTimeProviderContext ctx, ProviderSlot slot) =>
        slot.SlotId switch
        {
            0 => ResolveSession(ctx, slot.ProviderId),
            _ => throw new NotSupportedException()
        };

    public static object ResolveTransient(CompileTimeProviderContext ctx, ProviderSlot slot) =>
        slot.SlotId switch
        {
            0 => ResolveRepository(ctx, slot.ProviderId),
            _ => throw new NotSupportedException()
        };

    private static IClock ResolveIClock(CompileTimeProviderContext ctx, int providerId)
    {
        // IClock uses slot 0 for singleton in this part.
        return ctx.Root.GetOrCreate(
            new ProviderSlot(providerId, 0),
            ctx,
            static _ => new SystemClock());
    }

    private static Session ResolveSession(CompileTimeProviderContext ctx, int providerId)
    {
        return ctx.Current.GetOrCreate(
            new ProviderSlot(providerId, 0),
            ctx,
            static c =>
            {
                var session = new Session(c.ExternalDependencyResolver.GetRequiredService<ILogger>());
                c.Current.CaptureDisposable(session);
                return session;
            });
    }

    private static Repository ResolveRepository(CompileTimeProviderContext ctx, int providerId) =>
        new(ResolveSession(ctx, providerId));
}

[assembly: ExportsProviderPart(typeof(MyServiceProviderPart), singletonCount: 1, scopedCount: 1, transientCount: 1)]
```

### Generated root provider

```csharp
public sealed class AttributedDiServiceProvider : IServiceProvider, IServiceScopeFactory
{
    private static readonly FrozenDictionary<Type, ResolverToken> Lookup = ...;
    private readonly IServiceProvider _fallback;
    private readonly CompileTimeProviderContext _context;

    public AttributedDiServiceProvider(IServiceProvider fallback)
    {
        _fallback = fallback;
        _context = new(
            new ArrayBasedScopeCache(/* generated sizes */ []),
            new ArrayBasedScopeCache([]),
            this);
    }

    private AttributedDiServiceProvider(IScopeCache rootCache, IServiceProvider fallback)
    {
        _fallback = fallback;
        _context = new(rootCache, new DictionaryBasedScopeCache(32), this);
    }

    public object? GetService(Type serviceType)
    {
        if (!Lookup.TryGetValue(serviceType, out var token))
        {
            return _fallback.GetService(serviceType);
        }

        var slot = new ProviderSlot(token.ProviderId, token.SlotId);
        return token.Lifetime switch
        {
            Lifetime.Singleton => DispatchSingleton(_context, slot),
            Lifetime.Scoped => DispatchScoped(_context, slot),
            Lifetime.Transient => DispatchTransient(_context, slot),
            _ => throw new InvalidOperationException()
        };
    }

    public IServiceScope CreateScope() => new Scope(this, _fallback.CreateScope());

    // Dispatch methods are generated and switch by ProviderId.
    private sealed class Scope : IServiceScope, IDisposable, IAsyncDisposable
    {
        private readonly IServiceScope _fallbackScope;

        public Scope(AttributedDiServiceProvider root, IServiceScope fallbackScope)
        {
            _fallbackScope = fallbackScope;
            ServiceProvider = new AttributedDiServiceProvider(root._context.Root, fallbackScope.ServiceProvider);
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose() => _fallbackScope.Dispose();

        public ValueTask DisposeAsync()
        {
            if (_fallbackScope is IAsyncDisposable asyncDisposable)
            {
                return asyncDisposable.DisposeAsync();
            }

            _fallbackScope.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
```

## Resolution flow

1. Each part gets deterministic per-lifetime slots.
2. The entry project maps `Type` to `(Lifetime, ProviderId, SlotId)`.
3. `GetService(Type)` does one lookup, then dispatches by lifetime and provider.
4. In-part methods resolve local dependencies directly and use `ExternalDependencyResolver` for cross-part dependencies.

## Notes

- This can be fast for small/medium graphs.
- Very large switch trees may increase JIT cost and instruction-cache pressure.
- A chunked switch strategy is possible if service count becomes large. e.g.:

```csharp
public static class MyServiceProviderPart
{
    public static object ResolveSingleton(CompileTimeProviderContext ctx, ProviderSlot slot)
        => slot.SlotId switch
        {
            < 128 => ResolveGroup0(id),
            < 256 => ResolveGroup1(id),
            < 384 => ResolveGroup2(id),
            _ => ResolveGroup3(id)
        };
}
```

## MEDI comparison

TODO
