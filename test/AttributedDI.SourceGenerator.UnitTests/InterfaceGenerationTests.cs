namespace AttributedDI.SourceGenerator.UnitTests;

public class InterfaceGenerationTests
{
    [Fact]
    public async Task GeneratesInterfaceWithDefaultNamingAndSkipsDisposableMembers()
    {
        var code = """
                   using AttributedDI;
                   using System;

                   namespace MyApp
                   {
                       [GenerateInterface]
                       public class Foo : IDisposable
                       {
                           public void DoWork() { }

                           public void Dispose() { }
                       }
                   }
                   """;

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        await Verify(driver);
    }

    [Fact]
    public async Task GeneratesInterfaceWithCustomNameAndNamespace()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [GenerateInterface("ICustom", "Custom.Contracts")]
                       public class Bar
                       {
                           public int GetValue() => 42;
                       }
                   }
                   """;

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        await Verify(driver);
    }

    [Fact]
    public async Task RegistersTypeAgainstGeneratedInterface()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [RegisterAsGeneratedInterface("ICustomService", "Contracts")]
                       public class GeneratedService
                       {
                           public string Ping() => "pong";
                       }
                   }
                   """;

        var compilation = new CompilationFixture().WithSourceCode(code).Build();
        var driver = RunSourceGenerator(compilation, new ServiceRegistrationGenerator());

        await Verify(driver);
    }
}