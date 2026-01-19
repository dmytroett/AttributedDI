// See https://aka.ms/new-console-template for more information
using AllModulesRegistration.Core;
using AttributedDI;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();

services.AddAttributedDi();

var sp = services.BuildServiceProvider();

var internalService = sp.GetRequiredService<IInternalService>();

internalService.ThisIsInternal();

var external = sp.GetRequiredService<IMyAmazingService>();

external.HelloWorld();

[RegisterAsGeneratedInterface]
public partial class InternalService
{
    public void ThisIsInternal()
    {
        Console.WriteLine("This is from internal");
    }
}