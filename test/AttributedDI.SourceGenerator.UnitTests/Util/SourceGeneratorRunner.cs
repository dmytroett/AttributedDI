using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Globalization;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

public static class SourceGeneratorRunner
{
    public static GeneratorDriver RunSourceGenerator(CSharpCompilation compilation, params IIncrementalGenerator[] generators)
    {
        // Create an instance of our source generator
        GeneratorDriver driver = CSharpGeneratorDriver
            .Create(generators)
            .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics);

        // Run the source generator
        driver = driver.RunGenerators(compilation);

        var runResult = driver.GetRunResult();
        var generatedSyntaxTrees = runResult.GeneratedTrees;

        // Create a new compilation with both original and generated sources
        var compilationWithGenerated = compilation.AddSyntaxTrees(generatedSyntaxTrees);

        // Check for compilation errors
        var errors = compilationWithGenerated
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (errors.Count != 0)
        {
            var errorMessages = string.Join(
                Environment.NewLine,
                errors.Select(static e => $"{e.Id}: {e.GetMessage(CultureInfo.InvariantCulture)}"));
            Assert.Fail(errorMessages);
        }

        return driver;
    }
}