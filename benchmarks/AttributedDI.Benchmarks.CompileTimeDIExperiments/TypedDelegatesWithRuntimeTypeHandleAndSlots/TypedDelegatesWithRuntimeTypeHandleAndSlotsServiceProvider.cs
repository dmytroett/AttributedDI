using System.Collections.Frozen;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegatesWithRuntimeTypeHandleAndSlots;

internal readonly record struct TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext(
    TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider RootProvider,
    TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope? Scope,
    TypedDelegatesWithRuntimeTypeHandleAndSlotsRootCache RootCache,
    TypedDelegatesWithRuntimeTypeHandleAndSlotsScopedCache? ScopedCache)
{
    public T ResolveRequired<T>() where T : class
    {
        return RootProvider.GetRequiredService<T>(Scope);
    }

    public object ResolveRequired(Type serviceType)
    {
        return RootProvider.ResolveRequired(serviceType.TypeHandle, Scope, serviceType);
    }

    public T GetOrCreateSingleton<T>(
        int slot,
        Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        return RootCache.GetOrCreateSingleton(slot, this, factory);
    }

    public T GetOrCreateScoped<T>(
        int slot,
        Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
        where T : class
    {
        return Scope is null
            ? RootCache.GetOrCreateRootScoped(slot, this, factory)
            : ScopedCache!.GetOrCreate(slot, this, factory);
    }
}

internal sealed class TypedDelegatesWithRuntimeTypeHandleAndSlotsRootCache
{
    private readonly object _sync = new();
    private readonly object?[] _singletonInstances;
    private readonly object?[] _rootScopedInstances;

    public TypedDelegatesWithRuntimeTypeHandleAndSlotsRootCache(
        int singletonCount,
        int rootScopedCount)
    {
        _singletonInstances = new object?[singletonCount];
        _rootScopedInstances = new object?[rootScopedCount];
    }

    public T GetOrCreateSingleton<T>(
        int slot,
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context,
        Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
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
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context,
        Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
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

internal sealed class TypedDelegatesWithRuntimeTypeHandleAndSlotsScopedCache
{
    private readonly object?[] _instances;

    public TypedDelegatesWithRuntimeTypeHandleAndSlotsScopedCache(int scopedCount)
    {
        _instances = new object?[scopedCount];
    }

    public T GetOrCreate<T>(
        int slot,
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context,
        Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T> factory)
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

internal static class TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<T> where T : class
{
    public static Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider, TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, T>? Resolve;
}

internal sealed class TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider
    : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private static readonly RuntimeTypeHandle IServiceProviderTypeHandle = typeof(IServiceProvider).TypeHandle;
    private static readonly RuntimeTypeHandle IServiceScopeFactoryTypeHandle = typeof(IServiceScopeFactory).TypeHandle;

    private readonly FrozenDictionary<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>> _exports;
    private readonly TypedDelegatesWithRuntimeTypeHandleAndSlotsRootCache _rootCache;
    private readonly int _scopedSlotCount;
    private bool _disposed;

    public TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider(
        IEnumerable<KeyValuePair<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>> exports,
        int singletonSlotCount,
        int scopedSlotCount)
    {
        _exports = exports.ToFrozenDictionary();
        _rootCache = new TypedDelegatesWithRuntimeTypeHandleAndSlotsRootCache(
            singletonSlotCount,
            scopedSlotCount);
        _scopedSlotCount = scopedSlotCount;
    }

    public T GetRequiredService<T>() where T : class
    {
        return GetService<T>() ?? throw new InvalidOperationException($"No service registered for type '{typeof(T)}'.");
    }

    public T? GetService<T>() where T : class
    {
        return GetService<T>(CreateContext(scope: null));
    }

    public object? GetService(Type serviceType)
    {
        return ResolveService(serviceType.TypeHandle, scope: null);
    }

    public TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope CreateScope()
    {
        ThrowIfDisposed();
        return new TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope(this, _scopedSlotCount);
    }

    IServiceScope IServiceScopeFactory.CreateScope()
    {
        return CreateScope();
    }

    internal T? GetService<T>(TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
        where T : class
    {
        ThrowIfDisposed();

        var typedFactory = TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<T>.Resolve;
        if (typedFactory is not null)
        {
            return typedFactory(this, context);
        }

        return ResolveService(typeof(T).TypeHandle, context.Scope) as T;
    }

    internal T GetRequiredService<T>(TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope? scope)
        where T : class
    {
        return GetService<T>(CreateContext(scope))
            ?? throw new InvalidOperationException($"No service registered for type '{typeof(T)}'.");
    }

    internal object ResolveRequired(
        RuntimeTypeHandle serviceTypeHandle,
        TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope? scope,
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
        TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope? scope)
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

    internal TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext CreateContext(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope? scope)
    {
        return new TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext(
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
            nameof(TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider));
    }
}

internal sealed class TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope
    : IServiceScope, IServiceProvider
{
    private readonly TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider _rootProvider;
    private bool _disposed;

    public TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider rootProvider,
        int scopedSlotCount)
    {
        _rootProvider = rootProvider;
        ScopedCache = new TypedDelegatesWithRuntimeTypeHandleAndSlotsScopedCache(scopedSlotCount);
    }

    public IServiceProvider ServiceProvider => this;

    internal TypedDelegatesWithRuntimeTypeHandleAndSlotsScopedCache ScopedCache { get; }

    public T GetRequiredService<T>() where T : class
    {
        ThrowIfDisposed();
        return _rootProvider.GetRequiredService<T>(this);
    }

    public T? GetService<T>() where T : class
    {
        ThrowIfDisposed();
        return _rootProvider.GetService<T>(_rootProvider.CreateContext(this));
    }

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
            nameof(TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceScope));
    }
}
