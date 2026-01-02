using AttributedDI;

namespace GeneratedInterfacesSut;

[RegisterAsGeneratedInterface]
public partial class MyTransientClassToGenerateInterface
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}

[RegisterAsGeneratedInterface, Scoped]
public partial class MyScopedClassToGenerateInterface
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}

[RegisterAsGeneratedInterface]
public partial class ShouldNotGenerateInterfaceWithDisposable: IDisposable, IAsyncDisposable
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
public partial class ShouldGenerateInterfaceWithDisposableAndOtherMembers: IDisposable, IAsyncDisposable
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
public partial class GeneratesInterfaceButDoesntRegister
{
    public void PerformAction()
    {
        Console.WriteLine("Performing action...");
    }
}