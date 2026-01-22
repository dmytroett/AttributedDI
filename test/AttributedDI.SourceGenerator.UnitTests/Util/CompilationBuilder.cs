using Microsoft.CodeAnalysis;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

public class CompilationBuilder
{
    private readonly List<MetadataReference> _extraReferences = [];
    private readonly List<IIncrementalGenerator> _generators = [];
    private readonly Dictionary<string, string> _globalOptions = new(StringComparer.Ordinal);
    private string? _sourceCode;
    private string? _assemblyName;
    private OutputKind _outputKind = OutputKind.DynamicallyLinkedLibrary;
}

