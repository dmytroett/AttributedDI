using AttributedDI.SourceGenerator.AggregateServiceCollectionExtensionGeneration;
using AttributedDI.SourceGenerator.ServiceCollectionExtensionGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

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

        var diagnostics = await GetDiagnosticsAsync(code, new ConflictingExtensionNamespaceAnalyzer());

        Assert.Single(diagnostics);
        Assert.Equal("ATTDI003", diagnostics[0].Id);
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

        var diagnostics = await GetDiagnosticsAsync(code, new ConflictinglifetimeAnalyzer());

        Assert.Single(diagnostics);
        Assert.Equal("ATTDI001", diagnostics[0].Id);
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

        var optionsProvider = new FakeAnalyzerConfigOptionsProvider(
            ImmutableDictionary.CreateRange(
            [
                new KeyValuePair<string, string>("build_property.GenerateAttributedDIExtensions", "notabool"),
            ]));

        var diagnostics = await GetDiagnosticsAsync(
            code,
            new InvalidOptInMsbuildPropertyAnalyzer(),
            optionsProvider);

        Assert.Single(diagnostics);
        Assert.Equal("ATTDI002", diagnostics[0].Id);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
        string sourceCode,
        DiagnosticAnalyzer analyzer,
        AnalyzerConfigOptionsProvider? optionsProvider = null)
    {
        var compilation = CompilationFactory.CreateCompilation(
            sourceCode,
            assemblyName: "Tests",
            OutputKind.DynamicallyLinkedLibrary,
            extraReferences: Array.Empty<MetadataReference>());

        var analyzerOptions = optionsProvider is null
            ? new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty)
            : new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, optionsProvider);

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create(analyzer),
            analyzerOptions);

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
