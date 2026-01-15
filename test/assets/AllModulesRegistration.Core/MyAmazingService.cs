using AttributedDI;

namespace AllModulesRegistration.Core;

[RegisterAsGeneratedInterface]
public partial class MyAmazingService
{
    public void HelloWorld()
    {
        Console.WriteLine("Hello World");
    }
}