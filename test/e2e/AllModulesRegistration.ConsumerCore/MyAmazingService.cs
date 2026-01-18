using AttributedDI;

namespace AllModulesRegistration.ConsumerCore;

[RegisterAsGeneratedInterface]
public partial class MyAmazingService
{
    public void HelloWorld()
    {
        Console.WriteLine("Hello World");
    }
}
