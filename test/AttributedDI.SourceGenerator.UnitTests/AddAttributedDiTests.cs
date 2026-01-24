namespace AttributedDI.SourceGenerator.UnitTests;

public class AddAttributedDiTests
{
    [Fact]
    public async Task GeneratesAddAttributedDiWhenBuildPropertyIsSet()
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
                   """;

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", "true")
            .AddGenerator<AttributedDiSourceGenerator>()
            .BuildAndRun();

        Assert.Empty(result.Diagnostics);

        var output = GeneratedCodeExtractor.ExtractGeneratedCode(result);

        await Verify(output);
    }

    [Fact]
    public async Task GeneratesAddAttributedDiWhenNoRegisteredServicesExist()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", "true")
            .AddGenerator<AttributedDiSourceGenerator>()
            .BuildAndRun();

        Assert.Empty(result.Diagnostics);

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

        var referencedProject = await new CompilationTestFixture()
            .WithSourceCode(referencedSource)
            .WithAssemblyName("ReferencedAssembly")
            .AddGenerator<AttributedDiSourceGenerator>()
            .BuildAndRun();

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", "true")
            .WithReferencedProject(referencedProject)
            .AddGenerator<AttributedDiSourceGenerator>()
            .BuildAndRun();

        Assert.Empty(result.Diagnostics);

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

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", value)
            .AddGenerator<AttributedDiSourceGenerator>()
            .BuildAndRun();

        Assert.Empty(result.Diagnostics);

        var output = GeneratedCodeExtractor.ExtractGeneratedCode(result);

        await Verify(output);
    }
}
