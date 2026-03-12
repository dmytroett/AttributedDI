using System.Collections.Frozen;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandleAndSlots;

internal readonly record struct DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext(
    DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider RootProvider,
    DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceScope? Scope,
    DictionaryDelegatesWithRuntimeTypeHandleAndSlotsRootCache RootCache,
    DictionaryDelegatesWithRuntimeTypeHandleAndSlotsScopedCache? ScopedCache)
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
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        return RootCache.GetOrCreateSingleton(slot, this, factory);
    }

    public T GetOrCreateScoped<T>(
        int slot,
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        return Scope is null
            ? RootCache.GetOrCreateRootScoped(slot, this, factory)
            : ScopedCache!.GetOrCreate(slot, this, factory);
    }
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleAndSlotsRootCache
{
    private readonly object _sync = new();
    private readonly object?[] _singletonInstances;
    private readonly object?[] _rootScopedInstances;

    public DictionaryDelegatesWithRuntimeTypeHandleAndSlotsRootCache(
        int singletonCount,
        int rootScopedCount)
    {
        _singletonInstances = new object?[singletonCount];
        _rootScopedInstances = new object?[rootScopedCount];
    }

    public T GetOrCreateSingleton<T>(
        int slot,
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context,
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
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
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context,
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
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

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleAndSlotsScopedCache
{
    private readonly object?[] _instances;

    public DictionaryDelegatesWithRuntimeTypeHandleAndSlotsScopedCache(int scopedCount)
    {
        _instances = new object?[scopedCount];
    }

    public T GetOrCreate<T>(
        int slot,
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context,
        Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        var existing = _instances[slot];
        if (existing is not null)
        {
            return (T)existing;
        }

        var created = factory(context);
        _instances[slot] = created;
        return created;
    }

    public void Clear()
    {
        Array.Clear(_instances);
    }
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider
    : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private static readonly RuntimeTypeHandle IServiceProviderTypeHandle = typeof(IServiceProvider).TypeHandle;
    private static readonly RuntimeTypeHandle IServiceScopeFactoryTypeHandle = typeof(IServiceScopeFactory).TypeHandle;

    private readonly FrozenDictionary<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>> _exports;
    private readonly DictionaryDelegatesWithRuntimeTypeHandleAndSlotsRootCache _rootCache;
    private readonly int _scopedSlotCount;
    private bool _disposed;

    public DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider(
        IEnumerable<KeyValuePair<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>> exports,
        int singletonSlotCount,
        int scopedSlotCount)
    {
        _exports = exports.ToFrozenDictionary();
        _rootCache = new DictionaryDelegatesWithRuntimeTypeHandleAndSlotsRootCache(
            singletonSlotCount,
            scopedSlotCount);
        _scopedSlotCount = scopedSlotCount;
    }

    public object? GetService(Type serviceType)
    {
        return ResolveService(serviceType.TypeHandle, scope: null);
    }

    public DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceScope CreateScope()
    {
        ThrowIfDisposed();
        return new DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceScope(this, _scopedSlotCount);
    }

    IServiceScope IServiceScopeFactory.CreateScope()
    {
        return CreateScope();
    }

    internal object ResolveRequired(
        RuntimeTypeHandle serviceTypeHandle,
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceScope? scope,
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
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceScope? scope)
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

    internal DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext CreateContext(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceScope? scope)
    {
        return new DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext(
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
            nameof(DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider));
    }
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceScope
    : IServiceScope, IServiceProvider
{
    private readonly DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider _rootProvider;
    private bool _disposed;

    public DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceScope(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider rootProvider,
        int scopedSlotCount)
    {
        _rootProvider = rootProvider;
        ScopedCache = new DictionaryDelegatesWithRuntimeTypeHandleAndSlotsScopedCache(scopedSlotCount);
    }

    public IServiceProvider ServiceProvider => this;

    internal DictionaryDelegatesWithRuntimeTypeHandleAndSlotsScopedCache ScopedCache { get; }

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
            nameof(DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceScope));
    }
}
