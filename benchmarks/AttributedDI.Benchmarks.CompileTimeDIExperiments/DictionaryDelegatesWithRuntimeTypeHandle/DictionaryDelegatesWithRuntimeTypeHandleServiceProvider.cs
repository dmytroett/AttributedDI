using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;
using AttributedDI.Benchmarks.CompileTimeDIExperiments;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandle;

internal readonly record struct DictionaryDelegatesWithRuntimeTypeHandleServiceExport(
    RuntimeTypeHandle ServiceTypeHandle,
    Func<DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext, object> Factory);

internal readonly record struct DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext(
    DictionaryDelegatesWithRuntimeTypeHandleServiceProvider RootProvider,
    DictionaryDelegatesWithRuntimeTypeHandleServiceScope? Scope,
    IServiceCache<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext> SingletonCache,
    IServiceCache<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext> ScopedCache)
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

internal sealed class DictionaryDelegatesWithRuntimeTypeHandleServiceProvider
    : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private static readonly RuntimeTypeHandle IServiceProviderTypeHandle = typeof(IServiceProvider).TypeHandle;
    private static readonly RuntimeTypeHandle IServiceScopeFactoryTypeHandle = typeof(IServiceScopeFactory).TypeHandle;

    private readonly FrozenDictionary<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleServiceExport> _exports;
    private readonly ConcurrentRootServiceCache<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext> _singletonCache;
    private readonly ConcurrentRootServiceCache<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext> _rootScopedCache;
    private bool _disposed;

    public DictionaryDelegatesWithRuntimeTypeHandleServiceProvider(
        IEnumerable<DictionaryDelegatesWithRuntimeTypeHandleServiceExport> exports)
    {
        _exports = exports.ToFrozenDictionary(static export => export.ServiceTypeHandle);
        _singletonCache =
            new ConcurrentRootServiceCache<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext>();
        _rootScopedCache =
            new ConcurrentRootServiceCache<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext>();
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

        _singletonCache.Clear();
        _rootScopedCache.Clear();
        _disposed = true;
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

    internal IServiceCache<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext> ScopedCache { get; } =
        new LocklessScopedServiceCache<RuntimeTypeHandle, DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext>();

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
