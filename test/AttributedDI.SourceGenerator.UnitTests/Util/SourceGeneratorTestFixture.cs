using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Reflection;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

public class CompilationTestFixture
{
    private readonly List<MetadataReference> _extraReferences = [];
    private readonly List<IIncrementalGenerator> _generators = [];
    private readonly List<DiagnosticAnalyzer> _analyzers = [];
    private readonly Dictionary<string, string> _globalOptions = new(StringComparer.Ordinal);
    private string? _sourceCode;
    private string? _assemblyName;
    private OutputKind _outputKind = OutputKind.DynamicallyLinkedLibrary;

    public CompilationTestFixture WithExtraReferences(params MetadataReference[] references)
    {
        _extraReferences.AddRange(references);
        return this;
    }

    public CompilationTestFixture WithExtraReferences(params Assembly[] assemblies)
    {
        var references = assemblies.Select(a => MetadataReference.CreateFromFile(a.Location));
        _extraReferences.AddRange(references);
        return this;
    }

    public CompilationTestFixture WithSourceCode(string sourceCode)
    {
        _sourceCode = sourceCode;
        return this;
    }

    public CompilationTestFixture WithAssemblyName(string assemblyName)
    {
        _assemblyName = assemblyName;
        return this;
    }

    public CompilationTestFixture WithOutputKind(OutputKind outputKind)
    {
        _outputKind = outputKind;
        return this;
    }

    public CompilationTestFixture WithBuildProperty(string propertyName, string value)
    {
        _globalOptions[$"build_property.{propertyName}"] = value;
        return this;
    }

    public CompilationTestFixture WithReferencedProject(CompilationResult referencedCompilation)
    {
        var reference = AssemblyEmitter.EmitReference(referencedCompilation.UpdatedCompilation, referencedCompilation.UpdatedCompilation.AssemblyName);
        _extraReferences.Add(reference);
        return this;
    }

    public CompilationTestFixture AddGenerator<TGenerator>()
        where TGenerator : IIncrementalGenerator, new()
    {
        _generators.Add(new TGenerator());
        return this;
    }

    public CompilationTestFixture AddGenerators(params IIncrementalGenerator[] generators)
    {
        _generators.AddRange(generators);
        return this;
    }

    public CompilationTestFixture AddAnalyzer<TAnalyzer>()
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        _analyzers.Add(new TAnalyzer());
        return this;
    }

    public CompilationTestFixture AddAnalyzers(params DiagnosticAnalyzer[] analyzers)
    {
        _analyzers.AddRange(analyzers);
        return this;
    }

    public async Task<CompilationResult> BuildAndRun()
    {
        var compilation = CompilationFactory.CreateCompilation(
            _sourceCode,
            _assemblyName,
            _outputKind,
            _extraReferences);

        CompilationAssertions.AssertCompiles(compilation, "Pre-generators");

        var optionsProvider = GetOptionsProvider();
        var outputCompilation = GeneratorRunner.RunGenerators(
            compilation,
            _generators,
            optionsProvider,
            out var postGeneratorDiagnostics);

        CompilationAssertions.AssertCompiles(outputCompilation, "Post-generators");

        var analyzerDiagnostics = await RunAnalyzersAsync(outputCompilation, optionsProvider);
        var combinedDiagnostics = postGeneratorDiagnostics.AddRange(analyzerDiagnostics);

        return new CompilationResult(
            compilation,
            outputCompilation,
            combinedDiagnostics);
    }

    private FakeAnalyzerConfigOptionsProvider? GetOptionsProvider()
    {
        if (_globalOptions.Count == 0)
        {
            return null;
        }

        return new FakeAnalyzerConfigOptionsProvider(_globalOptions.ToImmutableDictionary());
    }

    private async Task<ImmutableArray<Diagnostic>> RunAnalyzersAsync(
        CSharpCompilation compilation,
        AnalyzerConfigOptionsProvider? optionsProvider)
    {
        if (_analyzers.Count == 0)
        {
            return ImmutableArray<Diagnostic>.Empty;
        }

        var analyzerOptions = optionsProvider is null
            ? new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty)
            : new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, optionsProvider);

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.CreateRange(_analyzers),
            analyzerOptions);

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}

public record CompilationResult(
    CSharpCompilation OriginalCompilation,
    CSharpCompilation UpdatedCompilation,
    ImmutableArray<Diagnostic> Diagnostics);
