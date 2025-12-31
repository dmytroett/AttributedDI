using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AttributedDI.SourceGenerator.UnitTests;

public static class SourceGeneratorRunner
{
    public static GeneratorDriver RunSourceGenerator(CSharpCompilation compilation, params IIncrementalGenerator[] generators)
    {
        // Create an instance of our source generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators);

        // Run the source generator
        driver = driver.RunGenerators(compilation);

        return driver;
    }
}