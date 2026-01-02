using AttributedDI;

namespace GeneratedInterfacesSut;

[RegisterAsGeneratedInterface]
public class MyClassToGenerateInterface
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}

[RegisterAsGeneratedInterface]
public class ShouldNotGenerateInterfaceWithDisposable: IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        // Dispose resources
    }

    public ValueTask DisposeAsync()
    {
        // Async dispose resources
        return ValueTask.CompletedTask;
    }
}

[RegisterAsGeneratedInterface]
public class ShouldGenerateInterfaceWithDisposableAndOtherMembers: IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        // Dispose resources
    }

    public ValueTask DisposeAsync()
    {
        // Async dispose resources
        return ValueTask.CompletedTask;
    }
}