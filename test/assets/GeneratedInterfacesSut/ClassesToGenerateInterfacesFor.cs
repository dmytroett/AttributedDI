using AttributedDI;

namespace GeneratedInterfacesSut;

[RegisterAsGeneratedInterface]
public class MyTransientClassToGenerateInterface
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}

[RegisterAsGeneratedInterface, Scoped]
public class MyScopedClassToGenerateInterface
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

[GenerateInterface]
public class GeneratesInterfaceButDoesntRegister
{
    public void PerformAction()
    {
        Console.WriteLine("Performing action...");
    }
}