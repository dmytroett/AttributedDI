using System.Collections.Frozen;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots;

internal readonly record struct DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext(
    DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProvider RootProvider,
    DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceScope? Scope,
    DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsRootCache RootCache,
    DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsScopedCache? ScopedCache)
{
    public T ResolveRequired<T>() where T : class
    {
        return (T)ResolveRequired(typeof(T));
    }

    public object ResolveRequired(Type serviceType)
    {
        return RootProvider.ResolveRequired(serviceType.TypeHandle, Scope, serviceType);
    }

    public T GetOrCreateSingleton<T>(
        int slot,
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        return RootCache.GetOrCreateSingleton(slot, this, factory);
    }

    public T GetOrCreateScoped<T>(
        RuntimeTypeHandle serviceTypeHandle,
        int rootSlot,
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        return Scope is null
            ? RootCache.GetOrCreateRootScoped(rootSlot, this, factory)
            : ScopedCache!.GetOrCreate(serviceTypeHandle, this, factory);
    }
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsRootCache
{
    private readonly object _sync = new();
    private readonly object?[] _singletonInstances;
    private readonly object?[] _rootScopedInstances;

    public DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsRootCache(
        int singletonCount,
        int rootScopedCount)
    {
        _singletonInstances = new object?[singletonCount];
        _rootScopedInstances = new object?[rootScopedCount];
    }

    public T GetOrCreateSingleton<T>(
        int slot,
        DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext context,
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        var existing = Volatile.Read(ref _singletonInstances[slot]);
        if (existing is not null)
        {
            return (T)existing;
        }

        lock (_sync)
        {
            existing = _singletonInstances[slot];
            if (existing is not null)
            {
                return (T)existing;
            }

            var created = factory(context);
            _singletonInstances[slot] = created;
            return created;
        }
    }

    public T GetOrCreateRootScoped<T>(
        int slot,
        DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext context,
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        var existing = Volatile.Read(ref _rootScopedInstances[slot]);
        if (existing is not null)
        {
            return (T)existing;
        }

        lock (_sync)
        {
            existing = _rootScopedInstances[slot];
            if (existing is not null)
            {
                return (T)existing;
            }

            var created = factory(context);
            _rootScopedInstances[slot] = created;
            return created;
        }
    }

    public void Clear()
    {
        Array.Clear(_singletonInstances);
        Array.Clear(_rootScopedInstances);
    }
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsScopedCache
{
    private readonly Dictionary<RuntimeTypeHandle, object> _instances = [];

    public T GetOrCreate<T>(
        RuntimeTypeHandle serviceTypeHandle,
        DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext context,
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        if (_instances.TryGetValue(serviceTypeHandle, out var existing))
        {
            return (T)existing;
        }

        var created = factory(context);
        _instances[serviceTypeHandle] = created;
        return created;
    }

    public void Clear()
    {
        _instances.Clear();
    }
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProvider
    : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private static readonly RuntimeTypeHandle IServiceProviderTypeHandle = typeof(IServiceProvider).TypeHandle;
    private static readonly RuntimeTypeHandle IServiceScopeFactoryTypeHandle = typeof(IServiceScopeFactory).TypeHandle;

    private readonly FrozenDictionary<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext, object>> _exports;
    private readonly DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsRootCache _rootCache;
    private bool _disposed;

    public DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProvider(
        IEnumerable<KeyValuePair<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext, object>>> exports,
        int singletonSlotCount,
        int rootScopedSlotCount)
    {
        _exports = exports.ToFrozenDictionary();
        _rootCache = new DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsRootCache(
            singletonSlotCount,
            rootScopedSlotCount);
    }

    public object? GetService(Type serviceType)
    {
        return ResolveService(serviceType.TypeHandle, scope: null);
    }

    public IServiceScope CreateScope()
    {
        ThrowIfDisposed();
        return new DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceScope(this);
    }

    internal object ResolveRequired(
        RuntimeTypeHandle serviceTypeHandle,
        DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceScope? scope,
        Type? serviceType = null)
    {
        var service = ResolveService(serviceTypeHandle, scope);
        if (service is not null)
        {
            return service;
        }

        throw new InvalidOperationException(
            $"No service registered for type '{serviceType ?? Type.GetTypeFromHandle(serviceTypeHandle)!}'.");
    }

    internal object? ResolveService(
        RuntimeTypeHandle serviceTypeHandle,
        DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceScope? scope)
    {
        ThrowIfDisposed();

        if (serviceTypeHandle.Equals(IServiceProviderTypeHandle))
        {
            return (IServiceProvider?)scope ?? this;
        }

        if (serviceTypeHandle.Equals(IServiceScopeFactoryTypeHandle))
        {
            return this;
        }

        if (!_exports.TryGetValue(serviceTypeHandle, out var factory))
        {
            return null;
        }

        return factory(CreateContext(scope));
    }

    internal DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext CreateContext(
        DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceScope? scope)
    {
        return new DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsCompileTimeProviderContext(
            this,
            scope,
            _rootCache,
            scope?.ScopedCache);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _rootCache.Clear();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            nameof(DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProvider));
    }
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceScope
    : IServiceScope, IServiceProvider
{
    private readonly DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProvider _rootProvider;
    private bool _disposed;

    public DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceScope(
        DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public IServiceProvider ServiceProvider => this;

    internal DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsScopedCache ScopedCache { get; } = new();

    public object? GetService(Type serviceType)
    {
        ThrowIfDisposed();
        return _rootProvider.ResolveService(serviceType.TypeHandle, this);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        ScopedCache.Clear();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            nameof(DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceScope));
    }
}
