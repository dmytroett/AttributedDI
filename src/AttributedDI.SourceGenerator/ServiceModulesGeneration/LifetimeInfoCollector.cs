using Microsoft.CodeAnalysis;

namespace AttributedDI.SourceGenerator.ServiceModulesGeneration;

internal static class LifetimeInfoCollector
{
    internal static IncrementalValuesProvider<LifetimeInfo> Collect(IncrementalGeneratorInitializationContext context)
    {
        var transient = CreateLifetimeCollector(context, KnownAttributes.TransientAttribute, "Transient");
        var scoped = CreateLifetimeCollector(context, KnownAttributes.ScopedAttribute, "Scoped");
        var singleton = CreateLifetimeCollector(context, KnownAttributes.SingletonAttribute, "Singleton");

        return AggregateIncrementalProviders(transient, scoped, singleton);
    }

    private static IncrementalValuesProvider<LifetimeInfo> CreateLifetimeCollector(
        IncrementalGeneratorInitializationContext context,
        string attributeMetadataName,
        string lifetime)
    {
        return context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: attributeMetadataName,
                predicate: static (_, _) => true,
                transform: (ctx, _) => ExtractLifetimeInfo(ctx, lifetime))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);
    }

    private static LifetimeInfo? ExtractLifetimeInfo(GeneratorAttributeSyntaxContext context, string lifetime)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return null;
        }

        return new LifetimeInfo(
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            lifetime);
    }
}
