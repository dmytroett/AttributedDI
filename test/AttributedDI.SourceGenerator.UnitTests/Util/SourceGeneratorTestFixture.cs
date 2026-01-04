using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

    public SourceGeneratorTestFixture WithReferencedAssemblySource(string sourceCode, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            GetBaseReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        AssertCodeCompiles(compilation, $"Reference assembly ({assemblyName})");

        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        if (!emitResult.Success)
        {
            string errorMessages = string.Join(
                Environment.NewLine,
                emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => $"  {d.GetMessage(CultureInfo.InvariantCulture)}"));
            Assert.Fail($"Reference assembly ({assemblyName}) emit failed:{Environment.NewLine}{errorMessages}");
        }

        stream.Position = 0;
        _extraReferences.Add(MetadataReference.CreateFromStream(stream));

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

    public SourceGeneratorTestResult RunAndGetOutput()
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
        var compilation = CSharpCompilation.Create(
            _assemblyName ?? "Tests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(_outputKind));

        AssertCodeCompiles(compilation, "Pre-generators");

        // Andrew Lock pioneered this approach in StronglyTypedID:
        // https://github.com/andrewlock/StronglyTypedId/blob/6bd17db4a4b700eaad9e209baf41478cc3f0bbe9/test/StronglyTypedIds.Tests/TestHelpers.cs#L31

        var originalTreeCount = compilation.SyntaxTrees.Length;

        GeneratorDriver driver = CSharpGeneratorDriver
            .Create(_generators.ToArray())
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var postGeneratorDiagnostics);

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
}

public record SourceGeneratorTestResult(
    string Output,
    ImmutableArray<Diagnostic> Diagnostics);