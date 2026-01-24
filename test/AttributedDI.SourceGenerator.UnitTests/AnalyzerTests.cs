using AttributedDI.SourceGenerator.AggregateServiceCollectionExtensionGeneration;
using AttributedDI.SourceGenerator.ServiceCollectionExtensionGeneration;

namespace AttributedDI.SourceGenerator.UnitTests;

public class AnalyzerTests
{
    [Fact]
    public async Task ConflictingExtensionNamespaceAnalyzer_ReportsConflict()
    {
        var code = """
                   using AttributedDI;

                   [assembly: ServiceCollectionExtension(
                       extensionClassName: "MyApp.Generated.MyExtensions",
                       extensionNamespace: "MyApp.Extensions")]

                   namespace MyApp
                   {
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictingExtensionNamespaceAnalyzer>()
            .BuildAndRun();

        Assert.Single(result.Diagnostics);
        Assert.Equal("ATTDI003", result.Diagnostics[0].Id);
    }

    [Fact]
    public async Task ConflictingExtensionNamespaceAnalyzer_DoesNotReportWhenNamespaceIsMissing()
    {
        var code = """
                   using AttributedDI;

                   [assembly: ServiceCollectionExtension(
                       extensionClassName: "MyApp.Generated.MyExtensions")]

                   namespace MyApp
                   {
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictingExtensionNamespaceAnalyzer>()
            .BuildAndRun();

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public async Task ConflictingExtensionNamespaceAnalyzer_DoesNotReportWhenClassNameIsNotQualified()
    {
        var code = """
                   using AttributedDI;

                   [assembly: ServiceCollectionExtension(
                       extensionClassName: "MyExtensions",
                       extensionNamespace: "MyApp.Generated")]

                   namespace MyApp
                   {
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictingExtensionNamespaceAnalyzer>()
            .BuildAndRun();

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public async Task ConflictingLifetimeAnalyzer_ReportsConflict()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [Transient]
                       [Scoped]
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictinglifetimeAnalyzer>()
            .BuildAndRun();

        Assert.Single(result.Diagnostics);
        Assert.Equal("ATTDI001", result.Diagnostics[0].Id);
    }

    [Fact]
    public async Task ConflictingLifetimeAnalyzer_DoesNotReportWhenSingleLifetime()
    {
        var code = """
                   using AttributedDI;

                   namespace MyApp
                   {
                       [Singleton]
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictinglifetimeAnalyzer>()
            .BuildAndRun();

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public async Task InvalidOptInMsbuildPropertyAnalyzer_ReportsInvalidValue()
    {
        var code = """
                   namespace MyApp
                   {
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", "notabool")
            .AddAnalyzer<InvalidOptInMsbuildPropertyAnalyzer>()
            .BuildAndRun();

        Assert.Single(result.Diagnostics);
        Assert.Equal("ATTDI002", result.Diagnostics[0].Id);
    }

    [Fact]
    public async Task InvalidOptInMsbuildPropertyAnalyzer_DoesNotReportForValidValue()
    {
        var code = """
                   namespace MyApp
                   {
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new SourceGeneratorTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", "true")
            .AddAnalyzer<InvalidOptInMsbuildPropertyAnalyzer>()
            .BuildAndRun();

        Assert.Empty(result.Diagnostics);
    }
}
