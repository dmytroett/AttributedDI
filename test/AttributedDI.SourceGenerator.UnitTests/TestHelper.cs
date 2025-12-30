using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AttributedDI.SourceGenerator.UnitTests;

public static class TestHelper
{
    public static Task CompileAndVerify(string source)
    {
        // Parse the provided string into a C# syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Get all necessary assembly references
        // Note: We only need AttributedDI reference since it's a minimal test
        // The compilation will still work for basic syntax analysis
        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(RegisterAsSelfAttribute).Assembly.Location)
        ];

        // Create a Roslyn compilation for the syntax tree with references
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references);

        // Create an instance of our source generator
        var generator = new ServiceRegistrationGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use Verify to snapshot test the source generator output!
        // UseDirectory organizes snapshots in a dedicated folder
        // Verify automatically uses: {ClassName}.{MethodName}#{GeneratedFileName}
        return Verify(driver).UseDirectory("Snapshots");
    }
}