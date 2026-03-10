using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandle;

internal readonly record struct DictionaryDelegatesWithRuntimeTypeHandleServiceExport(
    RuntimeTypeHandle ServiceTypeHandle,
    Func<DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext, object> Factory);

internal readonly record struct DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext(
    DictionaryDelegatesWithRuntimeTypeHandleServiceProvider RootProvider,
    DictionaryDelegatesWithRuntimeTypeHandleServiceScope? Scope,
    DictionaryDelegatesWithRuntimeTypeHandleServiceCache SingletonCache,
    DictionaryDelegatesWithRuntimeTypeHandleServiceCache ScopedCache)
{
    public T ResolveRequired<T>() where T : class
    {
        return (T)ResolveRequired(typeof(T));
    }

    public object ResolveRequired(Type serviceType)
    {
        return RootProvider.ResolveRequired(serviceType.TypeHandle, Scope, serviceType);
    }
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleServiceCache(object? sync = null)
{
    private readonly Dictionary<RuntimeTypeHandle, object> _instances = [];

    public T GetOrCreate<T>(
        RuntimeTypeHandle serviceTypeHandle,
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context,
        Func<DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext, T> factory)
        where T : class
    {
        if (sync is null)
        {
            return GetOrCreateCore(serviceTypeHandle, context, factory);
        }

        lock (sync)
        {
            return GetOrCreateCore(serviceTypeHandle, context, factory);
        }
    }

    public void Clear()
    {
        _instances.Clear();
    }

    private T GetOrCreateCore<T>(
        RuntimeTypeHandle serviceTypeHandle,
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context,
        Func<DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext, T> factory)
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
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleServiceProvider
    : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private static readonly RuntimeTypeHandle IServiceProviderTypeHandle = typeof(IServiceProvider).TypeHandle;
    private static readonly RuntimeTypeHandle IServiceScopeFactoryTypeHandle = typeof(IServiceScopeFactory).TypeHandle;

    private readonly FrozenDictionary<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleServiceExport> _exports;
    private readonly object _sync = new();
    private readonly DictionaryDelegatesWithRuntimeTypeHandleServiceCache _singletonCache;
    private readonly DictionaryDelegatesWithRuntimeTypeHandleServiceCache _rootScopedCache;
    private bool _disposed;

    public DictionaryDelegatesWithRuntimeTypeHandleServiceProvider(
        IEnumerable<DictionaryDelegatesWithRuntimeTypeHandleServiceExport> exports)
    {
        _exports = exports.ToFrozenDictionary(static export => export.ServiceTypeHandle);
        _singletonCache = new DictionaryDelegatesWithRuntimeTypeHandleServiceCache(_sync);
        _rootScopedCache = new DictionaryDelegatesWithRuntimeTypeHandleServiceCache(_sync);
    }

    public object? GetService(Type serviceType)
    {
        return ResolveService(serviceType.TypeHandle, scope: null);
    }

    public IServiceScope CreateScope()
    {
        ThrowIfDisposed();
        return new DictionaryDelegatesWithRuntimeTypeHandleServiceScope(this);
    }

    internal object ResolveRequired(
        RuntimeTypeHandle serviceTypeHandle,
        DictionaryDelegatesWithRuntimeTypeHandleServiceScope? scope,
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
        DictionaryDelegatesWithRuntimeTypeHandleServiceScope? scope)
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

        if (!_exports.TryGetValue(serviceTypeHandle, out var export))
        {
            return null;
        }

        return CreateService(export, scope);
    }

    internal DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext CreateContext(
        DictionaryDelegatesWithRuntimeTypeHandleServiceScope? scope)
    {
        return new DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext(
            this,
            scope,
            _singletonCache,
            scope?.ScopedCache ?? _rootScopedCache);
    }

    internal object CreateService(
        DictionaryDelegatesWithRuntimeTypeHandleServiceExport export,
        DictionaryDelegatesWithRuntimeTypeHandleServiceScope? scope)
    {
        return export.Factory(CreateContext(scope));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _singletonCache.Clear();
            _rootScopedCache.Clear();
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(DictionaryDelegatesWithRuntimeTypeHandleServiceProvider));
    }
}

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleServiceScope : IServiceScope, IServiceProvider
{
    private readonly DictionaryDelegatesWithRuntimeTypeHandleServiceProvider _rootProvider;
    private bool _disposed;

    public DictionaryDelegatesWithRuntimeTypeHandleServiceScope(
        DictionaryDelegatesWithRuntimeTypeHandleServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public IServiceProvider ServiceProvider => this;

    internal DictionaryDelegatesWithRuntimeTypeHandleServiceCache ScopedCache { get; } = new();

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
        ObjectDisposedException.ThrowIf(_disposed, nameof(DictionaryDelegatesWithRuntimeTypeHandleServiceScope));
    }
}
