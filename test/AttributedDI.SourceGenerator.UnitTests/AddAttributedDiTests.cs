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

        var result = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", "true")
            .AddGenerator<ServiceRegistrationGenerator>()
            .BuildAndRunGenerators();

        Assert.Empty(result.SourceGeneratorDiagnostics);

        var output = GeneratedCodeExtractor.ExtractGeneratedCode(result);

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

        var referencedProject = new SourceGeneratorTestFixture()
            .WithSourceCode(referencedSource)
            .WithAssemblyName("ReferencedAssembly")
            .AddGenerator<ServiceRegistrationGenerator>()
            .BuildAndRunGenerators();

        var result = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", "true")
            .WithReferencedProject(referencedProject)
            .AddGenerator<ServiceRegistrationGenerator>()
            .BuildAndRunGenerators();

        Assert.Empty(result.SourceGeneratorDiagnostics);

        var output =
            GeneratedCodeExtractor.ExtractGeneratedCode(referencedProject) +
            GeneratedCodeExtractor.ExtractGeneratedCode(result);

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

        var result = new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", value)
            .AddGenerator<ServiceRegistrationGenerator>()
            .BuildAndRunGenerators();

        Assert.Empty(result.SourceGeneratorDiagnostics);

        var output = GeneratedCodeExtractor.ExtractGeneratedCode(result);

        await Verify(output);
    }
}