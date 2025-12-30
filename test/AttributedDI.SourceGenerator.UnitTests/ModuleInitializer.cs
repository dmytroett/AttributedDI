using System.Runtime.CompilerServices;

namespace AttributedDI.SourceGenerator.UnitTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}