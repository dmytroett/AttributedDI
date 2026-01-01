using AttributedDI;

namespace Company.TeamName.Project.API;

public interface IRegisterAsInterfaceService
{
}

public interface IFirstService
{
}

public interface ISecondService
{
}

[RegisterAsSelf]
public class RegisterAsSelfTransientImplicitService
{
}

[RegisterAsSelf]
[Singleton]
public class RegisterAsSelfSingletonService
{
}

[RegisterAsSelf]
[Scoped]
public class RegisterAsSelfScopedService
{
}

[RegisterAs<IRegisterAsInterfaceService>]
[Scoped]
public class RegisterAsInterfaceScopedService : IRegisterAsInterfaceService
{
}

[RegisterAsImplementedInterfaces]
[Singleton]
public sealed class MultiInterfaceSingletonService : IFirstService, ISecondService, IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

[Transient]
public class LifetimeOnlyTransientService
{
}

// Keyed services
public interface IKeyedService
{
}

[RegisterAs<IKeyedService>("key1")]
[Singleton]
public class KeyedServiceOne : IKeyedService
{
}

[RegisterAs<IKeyedService>("key2")]
[Singleton]
public class KeyedServiceTwo : IKeyedService
{
}

[RegisterAsSelf("transientKey")]
public class RegisterAsSelfKeyedTransientService
{
}

[RegisterAsSelf("singletonKey")]
[Singleton]
public class RegisterAsSelfKeyedSingletonService
{
}