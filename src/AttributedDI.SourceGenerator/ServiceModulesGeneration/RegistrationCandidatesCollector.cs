using AttributedDI.SourceGenerator.InterfacesGeneration;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

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

        var implementationTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isOpenGeneric = IsOpenGenericDefinition(symbol);
        var unboundImplementationTypeName = ResolveUnboundName(symbol, implementationTypeName, isOpenGeneric);

        var candidate = new RegistrationCandidate(
            implementationTypeName,
            null,
            isOpenGeneric,
            unboundImplementationTypeName,
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

        var implementationTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isOpenGeneric = IsOpenGenericDefinition(symbol);
        var unboundImplementationTypeName = ResolveUnboundName(symbol, implementationTypeName, isOpenGeneric);

        var interfaces = symbol.AllInterfaces
            .Where(static iface => !WellKnownInterfacesRegistry.IsWellKnownInterface(iface))
            .Distinct(SymbolEqualityComparer.Default)
            .ToList();

        foreach (var iface in interfaces)
        {
            ct.ThrowIfCancellationRequested();

            builder.Add(new RegistrationCandidate(
                implementationTypeName,
                iface?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty,
                isOpenGeneric,
                unboundImplementationTypeName,
                iface is INamedTypeSymbol namedInterface
                    ? ResolveUnboundName(namedInterface, namedInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), namedInterface.IsGenericType)
                    : null,
                key));
        }

        if (builder.Count == 0)
        {
            builder.Add(new RegistrationCandidate(
                implementationTypeName,
                null,
                isOpenGeneric,
                unboundImplementationTypeName,
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

        var implementationTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isOpenGeneric = IsOpenGenericDefinition(symbol);
        var unboundImplementationTypeName = ResolveUnboundName(symbol, implementationTypeName, isOpenGeneric);

        foreach (var registerAsAttribute in context.Attributes)
        {
            ct.ThrowIfCancellationRequested();

            if (registerAsAttribute.AttributeClass is not { TypeArguments.Length: > 0 } attrClass)
            {
                continue;
            }

            var serviceType = attrClass.TypeArguments[0];
            var serviceTypeName = serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var unboundServiceTypeName = serviceType is INamedTypeSymbol namedService
                ? ResolveUnboundName(namedService, serviceTypeName, namedService.IsGenericType)
                : serviceTypeName;

            registrations.Add(new RegistrationCandidate(
                implementationTypeName,
                serviceTypeName,
                isOpenGeneric,
                unboundImplementationTypeName,
                unboundServiceTypeName,
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

        var implementationTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isOpenGeneric = IsOpenGenericDefinition(symbol);
        var unboundImplementationTypeName = ResolveUnboundName(symbol, implementationTypeName, isOpenGeneric);
        var serviceTypeFullName = BuildGeneratedInterfaceName(naming.FullyQualifiedName, symbol.TypeParameters.Length);
        var unboundServiceTypeFullName = symbol.TypeParameters.Length > 0 ? serviceTypeFullName : null;

        var candidate = new RegistrationCandidate(
            implementationTypeName,
            serviceTypeFullName,
            isOpenGeneric,
            unboundImplementationTypeName,
            unboundServiceTypeFullName,
            ExtractKey(attribute));

        return [candidate];
    }

    private static bool IsOpenGenericDefinition(INamedTypeSymbol symbol) => symbol.IsGenericType && symbol.IsDefinition;

    private static string ResolveUnboundName(INamedTypeSymbol symbol, string displayName, bool isGeneric)
    {
        if (!isGeneric)
        {
            return displayName;
        }

        var unbound = symbol.IsUnboundGenericType ? symbol : symbol.ConstructUnboundGenericType();
        return unbound.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static string BuildGeneratedInterfaceName(string fullyQualifiedName, int typeParameterCount)
    {
        var baseName = AddGlobalAlias(StripTypeParameters(fullyQualifiedName));

        if (typeParameterCount <= 0)
        {
            return baseName;
        }

        return $"{baseName}{BuildGenericAritySuffix(typeParameterCount)}";
    }

    private static string StripTypeParameters(string name)
    {
        var genericMarkerIndex = name.IndexOf('<');
        return genericMarkerIndex < 0 ? name : name[..genericMarkerIndex];
    }

    private static string BuildGenericAritySuffix(int typeParameterCount) => typeParameterCount switch
    {
        <= 0 => string.Empty,
        1 => "<>",
        _ => $"<{new string(',', typeParameterCount - 1)}>"
    };

    private static string AddGlobalAlias(string name)
    {
        return name.StartsWith("global::", StringComparison.Ordinal) ? name : $"global::{name}";
    }

    private static object? ExtractKey(AttributeData attribute)
    {
        var ctor = attribute.AttributeConstructor;
        if (ctor is { Parameters.Length: > 0 })
        {
            var keyParameterIndex = FindKeyParameterIndex(ctor);
            if (keyParameterIndex >= 0 && attribute.ConstructorArguments.Length > keyParameterIndex)
            {
                var keyArg = attribute.ConstructorArguments[keyParameterIndex];
                return !keyArg.IsNull ? keyArg.Value : null;
            }
        }

        var keyNamedArg = attribute.NamedArguments.FirstOrDefault(na => na.Key == "Key");
        return !keyNamedArg.Value.IsNull ? keyNamedArg.Value.Value : null;
    }

    private static int FindKeyParameterIndex(IMethodSymbol ctor)
    {
        for (var i = 0; i < ctor.Parameters.Length; i++)
        {
            if (string.Equals(ctor.Parameters[i].Name, "key", StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }
}