using Microsoft.CodeAnalysis;

namespace AttributedDI.SourceGenerator.UnitTests;

public class AddAttributedDiTests
{
    [Fact]
    public async Task GeneratesAddAttributedDiForEntryPoint()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [RegisterAsSelf]
                       public class MyService
                       {
                       }
                   }

                   public static class Program
                   {
                       public static void Main()
                       {
                       }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithOutputKind(OutputKind.ConsoleApplication)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Fact]
    public async Task IncludesReferencedGeneratedModules()
    {
        var referencedSource = """
                               using System;
                               using AttributedDI;
                               using Microsoft.Extensions.DependencyInjection;

                               namespace AttributedDI.Generated.Internal
                               {
                                   [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
                                   internal sealed class GeneratedModuleAttribute : Attribute
                                   {
                                   }
                               }

                               namespace ReferencedAssembly
                               {
                                   [AttributedDI.Generated.Internal.GeneratedModuleAttribute]
                                   public class ReferencedModule : IServiceModule
                                   {
                                       public void ConfigureServices(IServiceCollection services)
                                       {
                                       }
                                   }
                               }
                               """;

        var code = """
                   namespace MyApp
                   {
                       public class Dummy
                       {
                       }
                   }

                   public static class Program
                   {
                       public static void Main()
                       {
                       }
                   }
                   """;

        var (output, diagnostics) = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithOutputKind(OutputKind.ConsoleApplication)
            .WithReferencedAssemblySource(referencedSource, "ReferencedAssembly")
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }
}