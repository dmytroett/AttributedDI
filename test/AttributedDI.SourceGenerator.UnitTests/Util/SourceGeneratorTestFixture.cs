using AttributedDI.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Reflection;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

public class SourceGeneratorTestFixture
{
    private readonly List<MetadataReference> _extraReferences = [];
    private readonly List<IIncrementalGenerator> _generators = [];
    private readonly Dictionary<string, string> _globalOptions = new(StringComparer.Ordinal);
    private string? _sourceCode;
    private string? _assemblyName;
    private OutputKind _outputKind = OutputKind.DynamicallyLinkedLibrary;

    public SourceGeneratorTestFixture WithExtraReferences(params MetadataReference[] references)
    {
        _extraReferences.AddRange(references);
        return this;
    }

    public SourceGeneratorTestFixture WithExtraReferences(params Assembly[] assemblies)
    {
        var references = assemblies.Select(a => MetadataReference.CreateFromFile(a.Location));
        _extraReferences.AddRange(references);
        return this;
    }

    public SourceGeneratorTestFixture WithSourceCode(string sourceCode)
    {
        _sourceCode = sourceCode;
        return this;
    }

    public SourceGeneratorTestFixture WithAssemblyName(string assemblyName)
    {
        _assemblyName = assemblyName;
        return this;
    }

    public SourceGeneratorTestFixture WithOutputKind(OutputKind outputKind)
    {
        _outputKind = outputKind;
        return this;
    }

    public SourceGeneratorTestFixture WithBuildProperty(string propertyName, string value)
    {
        _globalOptions[$"build_property.{propertyName}"] = value;
        return this;
    }

    public SourceGeneratorTestFixture WithReferencedProject(CompilationResult referencedCompilation)
    {
        var reference = AssemblyEmitter.EmitReference(referencedCompilation.UpdatedCompilation, referencedCompilation.UpdatedCompilation.AssemblyName);
        _extraReferences.Add(reference);
        return this;
    }

    public SourceGeneratorTestFixture AddGenerator<TGenerator>()
        where TGenerator : IIncrementalGenerator, new()
    {
        _generators.Add(new TGenerator());
        return this;
    }

    public SourceGeneratorTestFixture AddGenerators(params IIncrementalGenerator[] generators)
    {
        _generators.AddRange(generators);
        return this;
    }

    public CompilationResult BuildAndRunGenerators()
    {
        var compilation = CompilationFactory.CreateCompilation(
            _sourceCode,
            _assemblyName,
            _outputKind,
            _extraReferences);

        CompilationAssertions.AssertCompiles(compilation, "Pre-generators");

        var outputCompilation = GeneratorRunner.RunGenerators(
            compilation,
            _generators,
            GetOptionsProvider(),
            out var postGeneratorDiagnostics);

        CompilationAssertions.AssertCompiles(outputCompilation, "Post-generators");

        return new CompilationResult(compilation, outputCompilation, postGeneratorDiagnostics);
    }

    private FakeAnalyzerConfigOptionsProvider? GetOptionsProvider()
    {
        if (_globalOptions.Count == 0)
        {
            return null;
        }

        return new FakeAnalyzerConfigOptionsProvider(_globalOptions.ToImmutableDictionary());
    }
}

public record CompilationResult(
    CSharpCompilation OriginalCompilation,
    CSharpCompilation UpdatedCompilation,
    ImmutableArray<Diagnostic> SourceGeneratorDiagnostics);