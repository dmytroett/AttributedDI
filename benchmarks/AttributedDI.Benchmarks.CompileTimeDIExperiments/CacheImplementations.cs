using System.Collections.Concurrent;
using System.Threading;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments;

internal interface IServiceCache<TKey, TContext> where TKey : notnull
{
    T GetOrCreate<T>(TKey serviceKey, TContext context, Func<TContext, T> factory)
        where T : class;

    void Clear();
}

internal sealed class LocklessScopedServiceCache<TKey, TContext> : IServiceCache<TKey, TContext> where TKey : notnull
{
    private readonly Dictionary<TKey, object> _instances = [];

    public T GetOrCreate<T>(TKey serviceKey, TContext context, Func<TContext, T> factory)
        where T : class
    {
        if (_instances.TryGetValue(serviceKey, out var existing))
        {
            return (T)existing;
        }

        var created = factory(context);
        _instances[serviceKey] = created;
        return created;
    }

    public void Clear()
    {
        _instances.Clear();
    }
}

internal sealed class ConcurrentRootServiceCache<TKey, TContext> : IServiceCache<TKey, TContext> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<object>> _instances = [];

    public T GetOrCreate<T>(TKey serviceKey, TContext context, Func<TContext, T> factory)
        where T : class
    {
        var lazy = _instances.GetOrAdd(
            serviceKey,
            _ => new Lazy<object>(
                () => factory(context),
                LazyThreadSafetyMode.ExecutionAndPublication));

        return (T)lazy.Value;
    }

    public void Clear()
    {
        _instances.Clear();
    }
}
