using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

internal static class GeneratorRunner
{
    public static CSharpCompilation RunGenerators(
        CSharpCompilation compilation,
        IReadOnlyList<IIncrementalGenerator> generators,
        AnalyzerConfigOptionsProvider? optionsProvider,
        out ImmutableArray<Diagnostic> postGeneratorDiagnostics)
    {
        var sourceGenerators = generators
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
}