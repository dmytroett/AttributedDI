using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointersWithRuntimeTypeHandle;

internal readonly struct DictionaryFunctionPointersWithRuntimeTypeHandleServiceExport
{
    public DictionaryFunctionPointersWithRuntimeTypeHandleServiceExport(
        RuntimeTypeHandle serviceTypeHandle,
        IntPtr factoryPointer)
    {
        ServiceTypeHandle = serviceTypeHandle;
        FactoryPointer = factoryPointer;
    }

    public RuntimeTypeHandle ServiceTypeHandle { get; }

    public IntPtr FactoryPointer { get; }
}

internal readonly record struct DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext(
    DictionaryFunctionPointersWithRuntimeTypeHandleServiceProvider RootProvider,
    DictionaryFunctionPointersWithRuntimeTypeHandleServiceScope? Scope,
    DictionaryFunctionPointersWithRuntimeTypeHandleServiceCache SingletonCache,
    DictionaryFunctionPointersWithRuntimeTypeHandleServiceCache ScopedCache)
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

internal sealed class DictionaryFunctionPointersWithRuntimeTypeHandleServiceCache(object? sync = null)
{
    private readonly Dictionary<RuntimeTypeHandle, object> _instances = [];

    public T GetOrCreate<T>(
        RuntimeTypeHandle serviceTypeHandle,
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context,
        Func<DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext, T> factory)
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
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context,
        Func<DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext, T> factory)
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

internal sealed class DictionaryFunctionPointersWithRuntimeTypeHandleServiceProvider
    : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private static readonly RuntimeTypeHandle IServiceProviderTypeHandle = typeof(IServiceProvider).TypeHandle;
    private static readonly RuntimeTypeHandle IServiceScopeFactoryTypeHandle = typeof(IServiceScopeFactory).TypeHandle;

    private readonly FrozenDictionary<RuntimeTypeHandle, DictionaryFunctionPointersWithRuntimeTypeHandleServiceExport> _exports;
    private readonly object _sync = new();
    private readonly DictionaryFunctionPointersWithRuntimeTypeHandleServiceCache _singletonCache;
    private readonly DictionaryFunctionPointersWithRuntimeTypeHandleServiceCache _rootScopedCache;
    private bool _disposed;

    public DictionaryFunctionPointersWithRuntimeTypeHandleServiceProvider(
        IEnumerable<DictionaryFunctionPointersWithRuntimeTypeHandleServiceExport> exports)
    {
        _exports = exports.ToFrozenDictionary(static export => export.ServiceTypeHandle);
        _singletonCache = new DictionaryFunctionPointersWithRuntimeTypeHandleServiceCache(_sync);
        _rootScopedCache = new DictionaryFunctionPointersWithRuntimeTypeHandleServiceCache(_sync);
    }

    public object? GetService(Type serviceType)
    {
        return ResolveService(serviceType.TypeHandle, scope: null);
    }

    public IServiceScope CreateScope()
    {
        ThrowIfDisposed();
        return new DictionaryFunctionPointersWithRuntimeTypeHandleServiceScope(this);
    }

    internal object ResolveRequired(
        RuntimeTypeHandle serviceTypeHandle,
        DictionaryFunctionPointersWithRuntimeTypeHandleServiceScope? scope,
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
        DictionaryFunctionPointersWithRuntimeTypeHandleServiceScope? scope)
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

    internal DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext CreateContext(
        DictionaryFunctionPointersWithRuntimeTypeHandleServiceScope? scope)
    {
        return new DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext(
            this,
            scope,
            _singletonCache,
            scope?.ScopedCache ?? _rootScopedCache);
    }

    internal unsafe object CreateService(
        DictionaryFunctionPointersWithRuntimeTypeHandleServiceExport export,
        DictionaryFunctionPointersWithRuntimeTypeHandleServiceScope? scope)
    {
        var context = CreateContext(scope);
        var factory =
            (delegate*<DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext, object>)export.FactoryPointer;
        return factory(context);
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
        ObjectDisposedException.ThrowIf(
            _disposed,
            nameof(DictionaryFunctionPointersWithRuntimeTypeHandleServiceProvider));
    }
}

internal sealed class DictionaryFunctionPointersWithRuntimeTypeHandleServiceScope : IServiceScope, IServiceProvider
{
    private readonly DictionaryFunctionPointersWithRuntimeTypeHandleServiceProvider _rootProvider;
    private bool _disposed;

    public DictionaryFunctionPointersWithRuntimeTypeHandleServiceScope(
        DictionaryFunctionPointersWithRuntimeTypeHandleServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public IServiceProvider ServiceProvider => this;

    internal DictionaryFunctionPointersWithRuntimeTypeHandleServiceCache ScopedCache { get; } = new();

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
            nameof(DictionaryFunctionPointersWithRuntimeTypeHandleServiceScope));
    }
}
