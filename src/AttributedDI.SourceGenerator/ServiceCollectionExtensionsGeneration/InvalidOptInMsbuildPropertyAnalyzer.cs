using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.ServiceCollectionExtensionsGeneration;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvalidOptInMsbuildPropertyAnalyzer : DiagnosticAnalyzer
{
    private const string GenerateExtensionsPropertyName = "GenerateAttributedDIExtensions";
    private static readonly DiagnosticDescriptor InvalidMsBuildPropertyValue = new(
        id: "ATTDI002",
        title: "Invalid MSBuild property value",
        messageFormat: "MSBuild property '{0}' has invalid value '{1}'. Expected 'true' or 'false'.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: ["CompilationEnd"]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [InvalidMsBuildPropertyValue];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationAction(ReportInvalidMsBuildPropertyValue);
    }

    private static void ReportInvalidMsBuildPropertyValue(CompilationAnalysisContext context)
    {
        var optionsProvider = context.Options.AnalyzerConfigOptionsProvider;

        if (!optionsProvider.GlobalOptions
            .TryGetValue($"build_property.{GenerateExtensionsPropertyName}", out var value))
        {
            return;
        }

        if (string.IsNullOrEmpty(value) || bool.TryParse(value, out _))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            InvalidMsBuildPropertyValue,
            Location.None,
            GenerateExtensionsPropertyName,
            value);
        context.ReportDiagnostic(diagnostic);
    }
}
