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
        // The compilation needs basic runtime references to properly resolve assembly-level attributes.
        // Without these, the source generator cannot read attributes like [assembly: RegistrationMethodName("...")]
        // because the compilation lacks the metadata for System.Attribute and related types.
        // See: https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.attributedata
        // See: https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md
        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib - provides System.Attribute
            MetadataReference.CreateFromFile(typeof(RegisterAsSelfAttribute).Assembly.Location), // AttributedDI assembly
            MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == "System.Runtime").Location) // System.Runtime - required for attribute metadata resolution
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