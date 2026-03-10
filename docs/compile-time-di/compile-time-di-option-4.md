# Compile-Time DI Option 4: Instance Provider Parts with Owned Caches

This option emits provider part instances (not static methods). Each instance owns the cache fields for the services it resolves.

## Public API surface

### Registration model

Registration input can match option 1 or option 2. The runtime shape differs by generating stateful part instances.

### Runtime contracts

```csharp
public interface IScopeCache : IDisposable, IAsyncDisposable
{
    object SyncRoot { get; }
    void CaptureDisposable(object instance); // used for disposable transients
}

public interface ICompiledServiceProvider<TService> : IDisposable, IAsyncDisposable
{
    TService GetService(IScopeCache scope, IServiceProvider externalDependencyResolver);
}
```

## Implementation details

### Generated singleton part

```csharp
public sealed class SingletonServicesProvider : ICompiledServiceProvider<IClock>
{
    private IClock? _clock;

    IClock ICompiledServiceProvider<IClock>.GetService(
        IScopeCache scope,
        IServiceProvider external) => GetClock(scope);

    private IClock GetClock(IScopeCache scope)
    {
        if (_clock is not null)
        {
            return _clock;
        }

        lock (scope.SyncRoot)
        {
            _clock ??= new SystemClock();
            return _clock;
        }
    }

    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
```

### Generated scoped/transient part

```csharp
public sealed class ScopedAndTransientServicesProvider :
    ICompiledServiceProvider<Session>,
    ICompiledServiceProvider<Repository>
{
    private Session? _session;

    Session ICompiledServiceProvider<Session>.GetService(
        IScopeCache scope,
        IServiceProvider external) => GetSession(scope, external);

    Repository ICompiledServiceProvider<Repository>.GetService(
        IScopeCache scope,
        IServiceProvider external) => GetRepository(scope, external);

    private Session GetSession(IScopeCache scope, IServiceProvider external)
    {
        if (_session is not null)
        {
            return _session;
        }

        lock (scope.SyncRoot)
        {
            if (_session is null)
            {
                _session = new Session(external.GetRequiredService<ILogger>());
            }

            return _session;
        }
    }

    private Repository GetRepository(IScopeCache scope, IServiceProvider external)
    {
        var repository = new Repository(GetSession(scope, external));
        // If Repository is disposable and transient, enlist it in scope.
        return repository;
    }

    public void Dispose() => _session?.Dispose();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
```

## Notes

- This design can reduce indirection by storing data directly in generated fields.
- It also introduces lifecycle complexity: root vs scope ownership, locking strategy, and disposal order.
- Exposing a lock object via `IScopeCache` is a design smell and likely needs refinement.

## MEDI comparison

TODO

