using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AttributedDI.SourceGenerator.InterfacesGeneration;

/// <summary>
/// Reports invalid interface generation usage and conflicting naming options.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InterfaceGenerationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor InvalidGenerateInterfaceUsage = new(
        id: "ATTDI004",
        title: "Invalid interface generation usage",
        messageFormat: "{0} can only be applied to non-nested partial class or struct types that can implement interfaces. '{1}' is {2}.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor ConflictingInterfaceNamespace = new(
        id: "ATTDI005",
        title: "Conflicting interface namespace",
        messageFormat: "{0} specifies a fully qualified interface name '{1}' and a namespace '{2}'. Remove the namespace or use a non-qualified interface name.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <summary>Gets the diagnostics supported by this analyzer.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [InvalidGenerateInterfaceUsage, ConflictingInterfaceNamespace];

    /// <summary>Registers analysis actions.</summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            var generateInterfaceAttribute = startContext.Compilation.GetTypeByMetadataName(
                KnownAttributes.GenerateInterfaceAttribute);
            var registerAsGeneratedInterfaceAttribute = startContext.Compilation.GetTypeByMetadataName(
                KnownAttributes.RegisterAsGeneratedInterfaceAttribute);

            if (generateInterfaceAttribute is null && registerAsGeneratedInterfaceAttribute is null)
            {
                return;
            }

            startContext.RegisterSymbolAction(
                symbolContext =>
                    AnalyzeNamedType(symbolContext, generateInterfaceAttribute, registerAsGeneratedInterfaceAttribute),
                SymbolKind.NamedType);
        });
    }

    private static void AnalyzeNamedType(
        SymbolAnalysisContext context,
        INamedTypeSymbol? generateInterfaceAttribute,
        INamedTypeSymbol? registerAsGeneratedInterfaceAttribute)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        foreach (var attributeData in typeSymbol.GetAttributes())
        {
            var attributeClass = attributeData.AttributeClass;
            if (attributeClass is null)
            {
                continue;
            }

            var isGenerateInterface = generateInterfaceAttribute is not null &&
                SymbolEqualityComparer.Default.Equals(attributeClass, generateInterfaceAttribute);
            var isRegisterAsGeneratedInterface = registerAsGeneratedInterfaceAttribute is not null &&
                SymbolEqualityComparer.Default.Equals(attributeClass, registerAsGeneratedInterfaceAttribute);

            if (!isGenerateInterface && !isRegisterAsGeneratedInterface)
            {
                continue;
            }

            ReportInvalidUsageIfNeeded(context, typeSymbol, attributeData, attributeClass);
            ReportConflictingNamespaceIfNeeded(context, attributeData, attributeClass);
        }
    }

    private static void ReportInvalidUsageIfNeeded(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        AttributeData attributeData,
        INamedTypeSymbol attributeClass)
    {
        var reasons = GetInvalidUsageReasons(typeSymbol);
        if (reasons.Count == 0)
        {
            return;
        }

        var location = attributeData.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
            ?? Location.None;
        var diagnostic = Diagnostic.Create(
            InvalidGenerateInterfaceUsage,
            location,
            attributeClass.Name,
            typeSymbol.Name,
            string.Join(", ", reasons));
        context.ReportDiagnostic(diagnostic);
    }

    private static void ReportConflictingNamespaceIfNeeded(
        SymbolAnalysisContext context,
        AttributeData attributeData,
        INamedTypeSymbol attributeClass)
    {
        GetAttributeValues(attributeData, out var interfaceName, out var interfaceNamespace);

        if (string.IsNullOrWhiteSpace(interfaceName) || string.IsNullOrWhiteSpace(interfaceNamespace))
        {
            return;
        }

        if (!IsFullyQualifiedName(interfaceName!))
        {
            return;
        }

        var location = attributeData.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
            ?? Location.None;
        var diagnostic = Diagnostic.Create(
            ConflictingInterfaceNamespace,
            location,
            attributeClass.Name,
            interfaceName,
            interfaceNamespace);
        context.ReportDiagnostic(diagnostic);
    }

    private static List<string> GetInvalidUsageReasons(INamedTypeSymbol typeSymbol)
    {
        var reasons = new List<string>();

        if (typeSymbol.TypeKind is not TypeKind.Class and not TypeKind.Struct)
        {
            reasons.Add("not a class or struct");
        }

        if (typeSymbol.ContainingType is not null)
        {
            reasons.Add("a nested type");
        }

        if (typeSymbol.IsStatic)
        {
            reasons.Add("a static class");
        }

        if (typeSymbol.IsRefLikeType)
        {
            reasons.Add("a ref struct");
        }

        if (!IsPartial(typeSymbol))
        {
            reasons.Add("not partial");
        }

        return reasons;
    }

    private static bool IsPartial(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.DeclaringSyntaxReferences.Length == 0)
        {
            return true;
        }

        foreach (var syntaxReference in typeSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is TypeDeclarationSyntax typeDeclaration &&
                !typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return false;
            }
        }

        return true;
    }

    private static void GetAttributeValues(
        AttributeData attributeData,
        out string? interfaceName,
        out string? interfaceNamespace)
    {
        interfaceName = GetNonEmptyConstructorArgument(attributeData, index: 0);
        interfaceNamespace = GetNonEmptyConstructorArgument(attributeData, index: 1);

        foreach (var namedArg in attributeData.NamedArguments)
        {
            if (namedArg.Key == "InterfaceName")
            {
                interfaceName = GetNonEmptyString(namedArg.Value);
            }
            else if (namedArg.Key == "InterfaceNamespace")
            {
                interfaceNamespace = GetNonEmptyString(namedArg.Value);
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

    private static bool IsFullyQualifiedName(string interfaceName)
    {
        const string globalPrefix = "global::";
        var normalized = interfaceName;
        if (normalized.StartsWith(globalPrefix, StringComparison.Ordinal))
        {
            normalized = normalized.Substring(globalPrefix.Length);
        }

        var genericMarkerIndex = normalized.IndexOf('<');
        if (genericMarkerIndex >= 0)
        {
            normalized = normalized.Substring(0, genericMarkerIndex);
        }

        return normalized.IndexOf('.') >= 0;
    }
}
