using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Linq;
using AttributedDI.SourceGenerator.InterfacesGeneration;

namespace AttributedDI.SourceGenerator.ServiceModulesGeneration;

internal static class RegistrationCandidatesCollector
{
    internal static IncrementalValuesProvider<RegistrationCandidate> CollectRegisterAsSelf(
        IncrementalGeneratorInitializationContext context)
    {
        return CreateAttributeCollector(context, KnownAttributes.RegisterAsSelfAttribute, ExtractRegisterAsSelf);
    }

    internal static IncrementalValuesProvider<RegistrationCandidate> CollectRegisterAsImplementedInterfaces(
        IncrementalGeneratorInitializationContext context)
    {
        return CreateAttributeCollector(context, KnownAttributes.RegisterAsImplementedInterfacesAttribute, ExtractRegisterAsImplementedInterfaces);
    }

    internal static IncrementalValuesProvider<RegistrationCandidate> CollectRegisterAs(
        IncrementalGeneratorInitializationContext context)
    {
        return CreateAttributeCollector(context, KnownAttributes.RegisterAsAttribute, ExtractRegisterAs);
    }

    internal static IncrementalValuesProvider<RegistrationCandidate> CollectRegisterAsGeneratedInterface(
        IncrementalGeneratorInitializationContext context)
    {
        return CreateAttributeCollector(context, KnownAttributes.RegisterAsGeneratedInterfaceAttribute, ExtractRegisterAsGeneratedInterface);
    }

    private static IncrementalValuesProvider<RegistrationCandidate> CreateAttributeCollector(
        IncrementalGeneratorInitializationContext context,
        string attributeMetadataName,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, ImmutableArray<RegistrationCandidate>> transform)
    {
        return context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: attributeMetadataName,
                predicate: static (_, _) => true,
                transform: transform)
            .SelectMany(static (candidates, _) => candidates);
    }

    private static ImmutableArray<RegistrationCandidate> ExtractRegisterAsSelf(
        GeneratorAttributeSyntaxContext context,
        CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return ImmutableArray<RegistrationCandidate>.Empty;
        }

        var candidate = new RegistrationCandidate(
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            null,
            ExtractKey(context.Attributes[0]));

        return [candidate];
    }

    private static ImmutableArray<RegistrationCandidate> ExtractRegisterAsImplementedInterfaces(
        GeneratorAttributeSyntaxContext context,
        CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return ImmutableArray<RegistrationCandidate>.Empty;
        }

        var builder = ImmutableArray.CreateBuilder<RegistrationCandidate>();
        var key = ExtractKey(context.Attributes[0]);

        var interfaces = symbol.AllInterfaces
            .Where(static iface => !WellKnownInterfacesRegistry.IsWellKnownInterface(iface))
            .Distinct(SymbolEqualityComparer.Default)
            .ToList();

        foreach (var iface in interfaces)
        {
            ct.ThrowIfCancellationRequested();

            builder.Add(new RegistrationCandidate(
                symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                iface?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty,
                key));
        }

        if (builder.Count == 0)
        {
            builder.Add(new RegistrationCandidate(
                symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                null,
                key));
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<RegistrationCandidate> ExtractRegisterAs(
        GeneratorAttributeSyntaxContext context,
        CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return ImmutableArray<RegistrationCandidate>.Empty;
        }

        var registrations = ImmutableArray.CreateBuilder<RegistrationCandidate>();

        foreach (var registerAsAttribute in context.Attributes)
        {
            ct.ThrowIfCancellationRequested();

            if (registerAsAttribute.AttributeClass is not { TypeArguments.Length: > 0 } attrClass)
            {
                continue;
            }

            var serviceType = attrClass.TypeArguments[0];

            registrations.Add(new RegistrationCandidate(
                symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                ExtractKey(registerAsAttribute)));
        }

        return registrations.ToImmutable();
    }

    private static ImmutableArray<RegistrationCandidate> ExtractRegisterAsGeneratedInterface(
        GeneratorAttributeSyntaxContext context,
        CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return ImmutableArray<RegistrationCandidate>.Empty;
        }

        var attribute = context.Attributes[0];
        if (!GeneratedInterfaceNamingResolver.TryResolve(symbol, attribute, out var naming) || naming is null)
        {
            // TODO: emit diagnostic when generated interface naming cannot be resolved.
            return ImmutableArray<RegistrationCandidate>.Empty;
        }

        var candidate = new RegistrationCandidate(
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            naming.FullyQualifiedName,
            ExtractKey(attribute));

        return [candidate];
    }

    private static object? ExtractKey(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 0)
        {
            var keyArg = attribute.ConstructorArguments[0];
            return !keyArg.IsNull ? keyArg.Value : null;
        }

        var keyNamedArg = attribute.NamedArguments.FirstOrDefault(na => na.Key == "Key");
        return !keyNamedArg.Value.IsNull ? keyNamedArg.Value.Value : null;
    }
}
