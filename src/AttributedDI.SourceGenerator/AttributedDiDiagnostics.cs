using Microsoft.CodeAnalysis;

namespace AttributedDI.SourceGenerator;

internal static class AttributedDiDiagnostics
{
    internal static readonly DiagnosticDescriptor InvalidMsBuildPropertyValue = new(
        id: "ATTDI002",
        title: "Invalid MSBuild property value",
        messageFormat: "MSBuild property '{0}' has invalid value '{1}'. Expected 'true' or 'false'.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: ["CompilationEnd"]);
}
