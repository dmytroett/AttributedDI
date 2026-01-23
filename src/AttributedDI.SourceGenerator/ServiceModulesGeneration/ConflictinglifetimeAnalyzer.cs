using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.ServiceModulesGeneration;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConflictinglifetimeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ConflictingLifetimes = new(
            id: "ATTDI001",
            title: "Conflicting lifetime attributes",
            messageFormat: "Type '{0}' has multiple lifetime attributes, use only one",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    /// <summary>Gets the diagnostics supported by this analyzer.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [ConflictingLifetimes];

    /// <summary>Registers analysis actions.</summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            var optionsProvider = startContext.Options.AnalyzerConfigOptionsProvider;

            var transientAttr = startContext.Compilation.GetTypeByMetadataName(KnownAttributes.TransientAttribute);
            var scopedAttr = startContext.Compilation.GetTypeByMetadataName(KnownAttributes.ScopedAttribute);
            var singletonAttr = startContext.Compilation.GetTypeByMetadataName(KnownAttributes.SingletonAttribute);

            startContext.RegisterSymbolAction(
                symbolContext =>
                {
                    if (transientAttr is null || scopedAttr is null || singletonAttr is null)
                    {
                        return;
                    }

                    AnalyzeNamedType(symbolContext, transientAttr, scopedAttr, singletonAttr);
                },
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
