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

