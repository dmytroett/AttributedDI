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

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictingExtensionNamespaceAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.ContainsConflictingExtensionNamespace(
            result.Diagnostics,
            "MyApp.Generated.MyExtensions",
            "MyApp.Extensions");
    }

    [Fact]
    public async Task ConflictingExtensionNamespaceAnalyzer_DoesNotReportWhenNamespaceIsMissing()
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
            .AddAnalyzer<ConflictingExtensionNamespaceAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.DoesNotContainConflictingExtensionNamespace(result.Diagnostics);
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

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictingExtensionNamespaceAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.DoesNotContainConflictingExtensionNamespace(result.Diagnostics);
    }

    [Fact]
    public async Task ConflictingExtensionNamespaceAnalyzer_DoesNotReportWhenQualifiedNameHasNoNamespaceOverride()
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

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictingExtensionNamespaceAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.DoesNotContainConflictingExtensionNamespace(result.Diagnostics);
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

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictinglifetimeAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.ContainsConflictingLifetime(result.Diagnostics, "MyService");
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

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<ConflictinglifetimeAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.DoesNotContainConflictingLifetime(result.Diagnostics);
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

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", "notabool")
            .AddAnalyzer<InvalidOptInMsbuildPropertyAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.ContainsInvalidMsBuildProperty(result.Diagnostics, "GenerateAttributedDIExtensions", "notabool");
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

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", "true")
            .AddAnalyzer<InvalidOptInMsbuildPropertyAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.DoesNotContainInvalidMsBuildProperty(result.Diagnostics);
    }

    [Fact]
    public async Task InvalidOptInMsbuildPropertyAnalyzer_DoesNotReportWhenPropertyMissing()
    {
        var code = """
                   namespace MyApp
                   {
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .AddAnalyzer<InvalidOptInMsbuildPropertyAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.DoesNotContainInvalidMsBuildProperty(result.Diagnostics);
    }

    [Fact]
    public async Task InvalidOptInMsbuildPropertyAnalyzer_DoesNotReportForEmptyValue()
    {
        var code = """
                   namespace MyApp
                   {
                       public class MyService
                       {
                       }
                   }
                   """;

        var result = await new CompilationTestFixture()
            .WithSourceCode(code)
            .WithBuildProperty("GenerateAttributedDIExtensions", string.Empty)
            .AddAnalyzer<InvalidOptInMsbuildPropertyAnalyzer>()
            .BuildAndRun();

        DiagnosticAssert.DoesNotContainInvalidMsBuildProperty(result.Diagnostics);
    }
}
