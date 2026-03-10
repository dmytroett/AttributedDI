using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;
using AttributedDI.Benchmarks.CompileTimeDIExperiments;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;

internal readonly record struct TypedDelegatesServiceExport(
    Type ServiceType,
    Func<TypedDelegatesCompileTimeProviderContext, object> Factory);

internal readonly record struct TypedDelegatesCompileTimeProviderContext(
    TypedDelegatesServiceProvider RootProvider,
    TypedDelegatesServiceScope? Scope,
    IServiceCache<Type, TypedDelegatesCompileTimeProviderContext> SingletonCache,
    IServiceCache<Type, TypedDelegatesCompileTimeProviderContext> ScopedCache)
{
    public T ResolveRequired<T>() where T : class
    {
        return RootProvider.GetRequiredService<T>(Scope);
    }

    public object ResolveRequired(Type serviceType)
    {
        return RootProvider.ResolveRequired(serviceType, Scope);
    }
}

internal static class TypedDelegatesCompiledResolver<T> where T : class
{
    public static Func<TypedDelegatesServiceProvider, TypedDelegatesCompileTimeProviderContext, T>? Resolve;
}

internal sealed class TypedDelegatesServiceProvider : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private readonly FrozenDictionary<Type, TypedDelegatesServiceExport> _exports;
    private readonly ConcurrentRootServiceCache<Type, TypedDelegatesCompileTimeProviderContext> _singletonCache;
    private readonly ConcurrentRootServiceCache<Type, TypedDelegatesCompileTimeProviderContext> _rootScopedCache;
    private bool _disposed;

    public TypedDelegatesServiceProvider(IEnumerable<TypedDelegatesServiceExport> exports)
    {
        _exports = exports.ToFrozenDictionary(static export => export.ServiceType);
        _singletonCache = new ConcurrentRootServiceCache<Type, TypedDelegatesCompileTimeProviderContext>();
        _rootScopedCache = new ConcurrentRootServiceCache<Type, TypedDelegatesCompileTimeProviderContext>();
    }

    public T GetRequiredService<T>() where T : class
    {
        return GetRequiredService<T>(scope: null);
    }

    public T? GetService<T>() where T : class
    {
        return GetService<T>(CreateContext(scope: null));
    }

    public object? GetService(Type serviceType)
    {
        return ResolveService(serviceType, scope: null);
    }

    public TypedDelegatesServiceScope CreateScope()
    {
        ThrowIfDisposed();
        return new TypedDelegatesServiceScope(this);
    }

    IServiceScope IServiceScopeFactory.CreateScope()
    {
        return CreateScope();
    }

    internal T? GetService<T>(TypedDelegatesCompileTimeProviderContext context) where T : class
    {
        ThrowIfDisposed();

        var typedFactory = TypedDelegatesCompiledResolver<T>.Resolve;
        if (typedFactory is not null)
        {
            return typedFactory(this, context);
        }

        return ResolveService(typeof(T), context.Scope) as T;
    }

    internal T GetRequiredService<T>(TypedDelegatesServiceScope? scope) where T : class
    {
        var service = GetService<T>(CreateContext(scope));
        if (service is not null)
        {
            return service;
        }

        throw new InvalidOperationException($"No service registered for type '{typeof(T)}'.");
    }

    internal object ResolveRequired(Type serviceType, TypedDelegatesServiceScope? scope)
    {
        var service = ResolveService(serviceType, scope);
        if (service is not null)
        {
            return service;
        }

        throw new InvalidOperationException($"No service registered for type '{serviceType}'.");
    }

    internal object? ResolveService(Type serviceType, TypedDelegatesServiceScope? scope)
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

        return export.Factory(CreateContext(scope));
    }

    internal TypedDelegatesCompileTimeProviderContext CreateContext(TypedDelegatesServiceScope? scope)
    {
        return new TypedDelegatesCompileTimeProviderContext(
            this,
            scope,
            _singletonCache,
            scope?.ScopedCache ?? _rootScopedCache);
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
        ObjectDisposedException.ThrowIf(_disposed, nameof(TypedDelegatesServiceProvider));
    }
}

internal sealed class TypedDelegatesServiceScope : IServiceScope, IServiceProvider
{
    private readonly TypedDelegatesServiceProvider _rootProvider;
    private bool _disposed;

    public TypedDelegatesServiceScope(TypedDelegatesServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public IServiceProvider ServiceProvider => this;

    internal IServiceCache<Type, TypedDelegatesCompileTimeProviderContext> ScopedCache { get; } =
        new LocklessScopedServiceCache<Type, TypedDelegatesCompileTimeProviderContext>();

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
        ObjectDisposedException.ThrowIf(_disposed, nameof(TypedDelegatesServiceScope));
    }
}
