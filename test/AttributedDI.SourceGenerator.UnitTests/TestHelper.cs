using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AttributedDI.SourceGenerator.UnitTests;

public static class TestHelper
{
    public static SettingsTask CompileAndVerify(string source)
    {
        // Parse the provided string into a C# syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create a Roslyn compilation for the syntax tree.
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree]);

        // Create an instance of our EnumGenerator incremental source generator
        var generator = new ServiceRegistrationGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verify(driver);
    }
}