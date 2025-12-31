using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Globalization;

namespace AttributedDI.SourceGenerator.UnitTests;

public class CompilationFixture
{
    private readonly List<MetadataReference> _extraReferences = [];
    private string? _sourceCode;

    public CompilationFixture WithExtraReferences(params MetadataReference[] references)
    {
        _extraReferences.AddRange(references);
        return this;
    }

    public CompilationFixture WithExtraReferences(params Type[] markerTypes)
    {
        var references = markerTypes.Select(t => MetadataReference.CreateFromFile(t.Assembly.Location));
        _extraReferences.AddRange(references);
        return this;
    }

    public CompilationFixture WithSourceCode(string sourceCode)
    {
        _sourceCode = sourceCode;
        return this;
    }

    public CSharpCompilation Build()
    {
        Debug.Assert(_sourceCode != null, $"{nameof(WithSourceCode)} has to be called to set source code");

        // Parse the provided string into a C# syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(_sourceCode);

        // Get all necessary assembly references
        // The compilation needs basic runtime references to properly resolve assembly-level attributes.
        // Without these, the source generator cannot read attributes like [assembly: RegistrationMethodName("...")]
        // because the compilation lacks the metadata for System.Attribute and related types.
        // See: https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md
        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly
                .Location), // System.Private.CoreLib - provides System.Attribute
            MetadataReference.CreateFromFile(typeof(RegisterAsSelfAttribute).Assembly
                .Location), // AttributedDI assembly
            MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == "System.Runtime")
                .Location), // System.Runtime - required for attribute metadata resolution
            .._extraReferences
        ];

        // Create a Roslyn compilation for the syntax tree with references
        var compilation = CSharpCompilation.Create(
            "Tests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Check for compilation errors before running the generator
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        if (errors.Count > 0)
        {
            string errorMessages = string.Join(Environment.NewLine,
                errors.Select(e => $"  {e.GetMessage(CultureInfo.InvariantCulture)}"));
            Assert.Fail($"Source code has compilation errors:{Environment.NewLine}{errorMessages}");
        }

        return compilation;
    }
}