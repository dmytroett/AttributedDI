using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AttributedDI.SourceGenerator.Analyzers;

/// <summary>Reports diagnostics for invalid AttributedDI registration usage.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RegistrationDiagnosticsAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ConflictingLifetimes = new(
        id: "ATTDI001",
        title: "Conflicting lifetime attributes",
        messageFormat: "Type '{0}' has multiple lifetime attributes, use only one",
        category: "AttributedDI",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostics supported by this analyzer.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(ConflictingLifetimes);

    /// <summary>Registers analysis actions.</summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            var transientAttr = startContext.Compilation.GetTypeByMetadataName("AttributedDI.TransientAttribute");
            var scopedAttr = startContext.Compilation.GetTypeByMetadataName("AttributedDI.ScopedAttribute");
            var singletonAttr = startContext.Compilation.GetTypeByMetadataName("AttributedDI.SingletonAttribute");

            if (transientAttr is null || scopedAttr is null || singletonAttr is null)
            {
                return;
            }

            startContext.RegisterSymbolAction(
                symbolContext => AnalyzeNamedType(symbolContext, transientAttr, scopedAttr, singletonAttr),
                SymbolKind.NamedType);
        });
    }

    private static void AnalyzeNamedType(
        SymbolAnalysisContext context,
        INamedTypeSymbol transientAttr,
        INamedTypeSymbol scopedAttr,
        INamedTypeSymbol singletonAttr)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        var hasTransient = HasAttribute(typeSymbol, transientAttr);
        var hasScoped = HasAttribute(typeSymbol, scopedAttr);
        var hasSingleton = HasAttribute(typeSymbol, singletonAttr);

        var lifetimeCount = (hasTransient ? 1 : 0) + (hasScoped ? 1 : 0) + (hasSingleton ? 1 : 0);
        if (lifetimeCount <= 1)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(ConflictingLifetimes, typeSymbol.Locations[0], typeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool HasAttribute(INamedTypeSymbol typeSymbol, INamedTypeSymbol attributeSymbol)
    {
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
            {
                return true;
            }
        }

        return false;
    }
}
