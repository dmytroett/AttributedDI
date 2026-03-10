using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;

internal readonly struct DictionaryFunctionPointersServiceExport
{
    public DictionaryFunctionPointersServiceExport(Type serviceType, IntPtr factoryPointer)
    {
        ServiceType = serviceType;
        FactoryPointer = factoryPointer;
    }

    public Type ServiceType { get; }

    public IntPtr FactoryPointer { get; }
}

internal readonly record struct DictionaryFunctionPointersCompileTimeProviderContext(
    DictionaryFunctionPointersServiceProvider RootProvider,
    DictionaryFunctionPointersServiceScope? Scope,
    DictionaryFunctionPointersServiceCache SingletonCache,
    DictionaryFunctionPointersServiceCache ScopedCache)
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

internal sealed class DictionaryFunctionPointersServiceCache(object? sync = null)
{
    private readonly Dictionary<Type, object> _instances = [];

    public T GetOrCreate<T>(
        Type serviceType,
        DictionaryFunctionPointersCompileTimeProviderContext context,
        Func<DictionaryFunctionPointersCompileTimeProviderContext, T> factory)
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
        DictionaryFunctionPointersCompileTimeProviderContext context,
        Func<DictionaryFunctionPointersCompileTimeProviderContext, T> factory)
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

internal sealed class DictionaryFunctionPointersServiceProvider
    : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private readonly FrozenDictionary<Type, DictionaryFunctionPointersServiceExport> _exports;
    private readonly object _sync = new();
    private readonly DictionaryFunctionPointersServiceCache _singletonCache;
    private readonly DictionaryFunctionPointersServiceCache _rootScopedCache;
    private bool _disposed;

    public DictionaryFunctionPointersServiceProvider(
        IEnumerable<DictionaryFunctionPointersServiceExport> exports)
    {
        _exports = exports.ToFrozenDictionary(static export => export.ServiceType);
        _singletonCache = new DictionaryFunctionPointersServiceCache(_sync);
        _rootScopedCache = new DictionaryFunctionPointersServiceCache(_sync);
    }

    public object? GetService(Type serviceType)
    {
        return ResolveService(serviceType, scope: null);
    }

    public IServiceScope CreateScope()
    {
        ThrowIfDisposed();
        return new DictionaryFunctionPointersServiceScope(this);
    }

    internal object ResolveRequired(Type serviceType, DictionaryFunctionPointersServiceScope? scope)
    {
        var service = ResolveService(serviceType, scope);
        if (service is not null)
        {
            return service;
        }

        throw new InvalidOperationException($"No service registered for type '{serviceType}'.");
    }

    internal object? ResolveService(Type serviceType, DictionaryFunctionPointersServiceScope? scope)
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

    internal DictionaryFunctionPointersCompileTimeProviderContext CreateContext(
        DictionaryFunctionPointersServiceScope? scope)
    {
        return new DictionaryFunctionPointersCompileTimeProviderContext(
            this,
            scope,
            _singletonCache,
            scope?.ScopedCache ?? _rootScopedCache);
    }

    internal unsafe object CreateService(
        DictionaryFunctionPointersServiceExport export,
        DictionaryFunctionPointersServiceScope? scope)
    {
        var context = CreateContext(scope);
        var factory = (delegate*<DictionaryFunctionPointersCompileTimeProviderContext, object>)export.FactoryPointer;
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
        ObjectDisposedException.ThrowIf(_disposed, nameof(DictionaryFunctionPointersServiceProvider));
    }
}

internal sealed class DictionaryFunctionPointersServiceScope : IServiceScope, IServiceProvider
{
    private readonly DictionaryFunctionPointersServiceProvider _rootProvider;
    private bool _disposed;

    public DictionaryFunctionPointersServiceScope(DictionaryFunctionPointersServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public IServiceProvider ServiceProvider => this;

    internal DictionaryFunctionPointersServiceCache ScopedCache { get; } = new();

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
        ObjectDisposedException.ThrowIf(_disposed, nameof(DictionaryFunctionPointersServiceScope));
    }
}
