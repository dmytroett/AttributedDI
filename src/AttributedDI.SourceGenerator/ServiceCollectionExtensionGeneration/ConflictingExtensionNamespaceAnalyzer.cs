using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.ServiceCollectionExtensionGeneration;

/// <summary>
/// Reports conflicts when a fully qualified extension class name and a namespace are both specified.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConflictingExtensionNamespaceAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ConflictingExtensionNamespace = new(
        id: "ATTDI003",
        title: "Conflicting extension namespace",
        messageFormat: "ServiceCollectionExtensionAttribute specifies a fully qualified extension class name '{0}' and a namespace '{1}'. Remove the namespace or use a non-qualified class name.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <summary>Gets the diagnostics supported by this analyzer.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [ConflictingExtensionNamespace];

    /// <summary>Registers analysis actions.</summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var attributeSymbol = context.Compilation.GetTypeByMetadataName(
            KnownAttributes.ServiceCollectionExtensionAttribute);
        if (attributeSymbol is null)
        {
            return;
        }

        foreach (var attributeData in context.Compilation.Assembly.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, attributeSymbol))
            {
                continue;
            }

            GetAttributeValues(attributeData, out var extensionClassName, out var extensionNamespace);

            if (string.IsNullOrWhiteSpace(extensionClassName) ||
                string.IsNullOrWhiteSpace(extensionNamespace))
            {
                continue;
            }

            if (!IsFullyQualifiedTypeName(extensionClassName!))
            {
                continue;
            }

            var location = attributeData.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
                ?? Location.None;
            var diagnostic = Diagnostic.Create(
                ConflictingExtensionNamespace,
                location,
                extensionClassName,
                extensionNamespace);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void GetAttributeValues(
        AttributeData attributeData,
        out string? extensionClassName,
        out string? extensionNamespace)
    {
        extensionClassName = GetNonEmptyConstructorArgument(attributeData, index: 0);
        extensionNamespace = GetNonEmptyConstructorArgument(attributeData, index: 2);

        foreach (var namedArg in attributeData.NamedArguments)
        {
            if (namedArg.Key == "ExtensionClassName")
            {
                extensionClassName = GetNonEmptyString(namedArg.Value);
            }
            else if (namedArg.Key == "ExtensionNamespace")
            {
                extensionNamespace = GetNonEmptyString(namedArg.Value);
            }
        }
    }

    private static string? GetNonEmptyConstructorArgument(AttributeData attributeData, int index)
    {
        if (attributeData.ConstructorArguments.Length <= index)
        {
            return null;
        }

        return GetNonEmptyString(attributeData.ConstructorArguments[index]);
    }

    private static string? GetNonEmptyString(TypedConstant constant)
    {
        if (!constant.IsNull && constant.Value is string value && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return null;
    }

    private static bool IsFullyQualifiedTypeName(string extensionClassName)
    {
        const string globalPrefix = "global::";
        var normalized = extensionClassName;
        if (normalized.StartsWith(globalPrefix, StringComparison.Ordinal))
        {
            normalized = normalized.Substring(globalPrefix.Length);
        }

        return normalized.IndexOf('.') >= 0;
    }
}
