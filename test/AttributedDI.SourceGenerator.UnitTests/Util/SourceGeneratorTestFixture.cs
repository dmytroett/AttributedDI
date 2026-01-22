using AttributedDI.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

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

    public SourceGeneratorTestFixture WithReferencedAssemblySource(string sourceCode, string assemblyName)
    {
        var referencedProject = new SourceGeneratorTestFixture()
            .WithSourceCode(sourceCode)
            .WithAssemblyName(assemblyName)
            .AddGenerator<ServiceRegistrationGenerator>();

        return WithReferencedProject(referencedProject);
    }

    public SourceGeneratorTestFixture WithReferencedProject(SourceGeneratorTestFixture referencedProject)
    {
        _extraReferences.Add(referencedProject.BuildReference());
        return this;
    }

    public CSharpCompilation BuildCompilation()
    {
        var compilation = CreateCompilation();
        AssertCodeCompiles(compilation, "Pre-generators");
        return compilation;
    }

    public CSharpCompilation BuildGeneratedCompilation()
    {
        var compilation = BuildCompilation();
        var outputCompilation = RunGenerators(compilation, out _);
        AssertCodeCompiles(outputCompilation, "Post-generators");
        return outputCompilation;
    }

    public PortableExecutableReference BuildReference()
    {
        var outputCompilation = BuildGeneratedCompilation();
        return EmitReference(outputCompilation, outputCompilation.AssemblyName);
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

    public SourceGeneratorTestResult RunAndGetOutput()
    {
        CSharpCompilation compilation = BuildCompilation();

        // Andrew Lock pioneered this approach in StronglyTypedID:
        // https://github.com/andrewlock/StronglyTypedId/blob/6bd17db4a4b700eaad9e209baf41478cc3f0bbe9/test/StronglyTypedIds.Tests/TestHelpers.cs#L31

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var outputCompilation = RunGenerators(compilation, out var postGeneratorDiagnostics);

        AssertCodeCompiles(outputCompilation, "Post-generators");

        var output = outputCompilation.SyntaxTrees
            .Skip(originalTreeCount)
            .Aggregate(new StringBuilder(), (sb, tree) =>
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"File name: {Path.GetFileName(tree.FilePath)}");
                sb.AppendLine(tree.ToString());

                return sb;
            })
            .ToString();

        return new SourceGeneratorTestResult(output, postGeneratorDiagnostics);
    }

    private CSharpCompilation CreateCompilation()
    {
        Debug.Assert(_sourceCode != null, $"{nameof(WithSourceCode)} has to be called to set source code");

        // Parse the provided string into a C# syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(_sourceCode);

        // Get all necessary assembly references
        // The compilation needs basic runtime references to properly resolve assembly-level attributes.
        // Without these, the source generator cannot read attributes like [assembly: RegistrationMethodName("...")]
        // because the compilation lacks the metadata for System.Attribute and related types.
        // See: https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md
        List<MetadataReference> references = [.. GetBaseReferences(), .. _extraReferences];

        // Create a Roslyn compilation for the syntax tree with references
        return CSharpCompilation.Create(
            _assemblyName ?? "Tests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(_outputKind));
    }

    private CSharpCompilation RunGenerators(CSharpCompilation compilation, out ImmutableArray<Diagnostic> postGeneratorDiagnostics)
    {
        AnalyzerConfigOptionsProvider? optionsProvider = null;

        if (_globalOptions.Count > 0)
        {
            optionsProvider = new TestAnalyzerConfigOptionsProvider(_globalOptions.ToImmutableDictionary());
        }

        var sourceGenerators = _generators
            .Select(static generator => generator.AsSourceGenerator())
            .ToArray();

        if (sourceGenerators.Length == 0)
        {
            postGeneratorDiagnostics = ImmutableArray<Diagnostic>.Empty;
            return compilation;
        }

        _ = CSharpGeneratorDriver
            .Create(sourceGenerators, optionsProvider: optionsProvider)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out postGeneratorDiagnostics);

        return (CSharpCompilation)outputCompilation;
    }

    private static PortableExecutableReference EmitReference(Compilation compilation, string? assemblyName)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        if (!emitResult.Success)
        {
            string errorMessages = string.Join(
                Environment.NewLine,
                emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => $"  {d.GetMessage(CultureInfo.InvariantCulture)}"));
            Assert.Fail($"Reference assembly ({assemblyName ?? "Unnamed"}) emit failed:{Environment.NewLine}{errorMessages}");
        }

        stream.Position = 0;
        return MetadataReference.CreateFromStream(stream);
    }

    private static List<MetadataReference> GetBaseReferences()
    {
        return
        [
            // System.Private.CoreLib - provides System.Attribute
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            // AttributedDI assembly
            MetadataReference.CreateFromFile(typeof(RegisterAsSelfAttribute).Assembly.Location), 
            // Microsoft.Extensions.DependencyInjection.Abstractions
            MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location), 
            // System.Runtime - required for attribute metadata resolution
            MetadataReference.CreateFromFile(
                AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "System.Runtime").Location),
        ];
    }

    private static void AssertCodeCompiles(Compilation compilation, string stageName)
    {
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        if (errors.Count > 0)
        {
            string errorMessages = string.Join(Environment.NewLine, errors.Select(e => $"  {e.GetMessage(CultureInfo.InvariantCulture)}"));
            Assert.Fail($"{stageName} source code has compilation errors:{Environment.NewLine}{errorMessages}");
        }
    }

    private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions EmptyOptions = new TestAnalyzerConfigOptions(ImmutableDictionary<string, string>.Empty);
        private readonly AnalyzerConfigOptions _globalOptions;

        public TestAnalyzerConfigOptionsProvider(ImmutableDictionary<string, string> globalOptions)
        {
            _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
        }

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return EmptyOptions;
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return EmptyOptions;
        }
    }

    private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly ImmutableDictionary<string, string> _options;

        public TestAnalyzerConfigOptions(ImmutableDictionary<string, string> options)
        {
            _options = options;
        }

        public override bool TryGetValue(string key, out string value)
        {
            if (_options.TryGetValue(key, out var storedValue))
            {
                value = storedValue;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}

public record SourceGeneratorTestResult(
    string Output,
    ImmutableArray<Diagnostic> Diagnostics);