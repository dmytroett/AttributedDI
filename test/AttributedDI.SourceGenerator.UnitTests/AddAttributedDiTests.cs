using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;

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
            .WithBuildProperty("GenerateAttributedDIExtensions", "true")
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

                               namespace ReferencedAssembly
                               {
                                   [RegisterAsSelf]
                                   public class FromReferenced
                                   {
                                   }
                               }
                               """;

        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [RegisterAsSelf]
                       public class FromSelf
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
            .WithBuildProperty("GenerateAttributedDIExtensions", "true")
            .WithReferencedAssemblySource(referencedSource, "ReferencedAssembly")
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("")]
    public async Task DoesNotGenerateAddAttributedDiWhenDisabledOrIncorrectValue(string value)
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
            .WithBuildProperty("GenerateAttributedDIExtensions", value)
            .AddGenerator<ServiceRegistrationGenerator>()
            .RunAndGetOutput();

        Assert.Empty(diagnostics);

        await Verify(output);
    }
}