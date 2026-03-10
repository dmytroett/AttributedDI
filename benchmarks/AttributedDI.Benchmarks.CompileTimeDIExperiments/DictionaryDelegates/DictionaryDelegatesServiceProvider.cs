using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;

internal readonly record struct DictionaryDelegatesServiceExport(
    Type ServiceType,
    Func<DictionaryDelegatesCompileTimeProviderContext, object> Factory);

internal readonly record struct DictionaryDelegatesCompileTimeProviderContext(
    DictionaryDelegatesServiceProvider RootProvider,
    DictionaryDelegatesServiceScope? Scope,
    DictionaryDelegatesServiceCache SingletonCache,
    DictionaryDelegatesServiceCache ScopedCache)
{
    public T ResolveRequired<T>() where T : class
    {
        return (T)ResolveRequired(typeof(T));
    }

    public object ResolveRequired(Type serviceType)
    {
        return RootProvider.ResolveRequired(serviceType, Scope);
    }
}

internal sealed class DictionaryDelegatesServiceCache(object? sync = null)
{
    private readonly Dictionary<Type, object> _instances = [];

    public T GetOrCreate<T>(
        Type serviceType,
        DictionaryDelegatesCompileTimeProviderContext context,
        Func<DictionaryDelegatesCompileTimeProviderContext, T> factory)
        where T : class
    {
        if (sync is null)
        {
            return GetOrCreateCore(serviceType, context, factory);
        }

        lock (sync)
        {
            return GetOrCreateCore(serviceType, context, factory);
        }
    }

    public void Clear()
    {
        _instances.Clear();
    }

    private T GetOrCreateCore<T>(
        Type serviceType,
        DictionaryDelegatesCompileTimeProviderContext context,
        Func<DictionaryDelegatesCompileTimeProviderContext, T> factory)
        where T : class
    {
        if (_instances.TryGetValue(serviceType, out var existing))
        {
            return (T)existing;
        }

        var created = factory(context);
        _instances[serviceType] = created;
        return created;
    }
}

internal sealed class DictionaryDelegatesServiceProvider : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private readonly FrozenDictionary<Type, DictionaryDelegatesServiceExport> _exports;
    private readonly object _sync = new();
    private readonly DictionaryDelegatesServiceCache _singletonCache;
    private readonly DictionaryDelegatesServiceCache _rootScopedCache;
    private bool _disposed;

    public DictionaryDelegatesServiceProvider(IEnumerable<DictionaryDelegatesServiceExport> exports)
    {
        _exports = exports.ToFrozenDictionary(static export => export.ServiceType);
        _singletonCache = new DictionaryDelegatesServiceCache(_sync);
        _rootScopedCache = new DictionaryDelegatesServiceCache(_sync);
    }

    public object? GetService(Type serviceType)
    {
        return ResolveService(serviceType, scope: null);
    }

    public IServiceScope CreateScope()
    {
        ThrowIfDisposed();
        return new DictionaryDelegatesServiceScope(this);
    }

    internal object ResolveRequired(Type serviceType, DictionaryDelegatesServiceScope? scope)
    {
        var service = ResolveService(serviceType, scope);
        if (service is not null)
        {
            return service;
        }

        throw new InvalidOperationException($"No service registered for type '{serviceType}'.");
    }

    internal object? ResolveService(Type serviceType, DictionaryDelegatesServiceScope? scope)
    {
        ThrowIfDisposed();

        if (serviceType == typeof(IServiceProvider))
        {
            return (IServiceProvider?)scope ?? this;
        }

        if (serviceType == typeof(IServiceScopeFactory))
        {
            return this;
        }

        if (!_exports.TryGetValue(serviceType, out var export))
        {
            return null;
        }

        return CreateService(export, scope);
    }

    internal DictionaryDelegatesCompileTimeProviderContext CreateContext(
        DictionaryDelegatesServiceScope? scope)
    {
        return new DictionaryDelegatesCompileTimeProviderContext(
            this,
            scope,
            _singletonCache,
            scope?.ScopedCache ?? _rootScopedCache);
    }

    internal object CreateService(
        DictionaryDelegatesServiceExport export,
        DictionaryDelegatesServiceScope? scope)
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
        ObjectDisposedException.ThrowIf(_disposed, nameof(DictionaryDelegatesServiceProvider));
    }
}

internal sealed class DictionaryDelegatesServiceScope : IServiceScope, IServiceProvider
{
    private readonly DictionaryDelegatesServiceProvider _rootProvider;
    private bool _disposed;

    public DictionaryDelegatesServiceScope(DictionaryDelegatesServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public IServiceProvider ServiceProvider => this;

    internal DictionaryDelegatesServiceCache ScopedCache { get; } = new();

    public object? GetService(Type serviceType)
    {
        ThrowIfDisposed();
        return _rootProvider.ResolveService(serviceType, this);
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
        ObjectDisposedException.ThrowIf(_disposed, nameof(DictionaryDelegatesServiceScope));
    }
}
