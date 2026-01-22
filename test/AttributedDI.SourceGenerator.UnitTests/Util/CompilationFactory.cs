using AttributedDI.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

internal static class CompilationFactory
{
    public static CSharpCompilation CreateCompilation(
        string? sourceCode,
        string? assemblyName,
        OutputKind outputKind,
        IEnumerable<MetadataReference> extraReferences)
    {
        Debug.Assert(sourceCode != null, "WithSourceCode has to be called to set source code");

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // The compilation needs runtime references to resolve assembly-level attributes
        // like [assembly: RegistrationMethodName("...")]. Without these, the generator
        // cannot read attribute metadata (System.Attribute, System.Runtime, etc.).
        List<MetadataReference> references = [.. GetBaseReferences(), .. extraReferences];

        return CSharpCompilation.Create(
            assemblyName ?? "Tests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(outputKind));
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
}