using AttributedDI.SourceGenerator.InterfacesGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.Globalization;
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

        if (symbol.ContainingType is not null)
        {
            // Nested types are not supported for generated interfaces/registrations.
            return ImmutableArray<RegistrationCandidate>.Empty;
        }

        var attribute = context.Attributes[0];
        var naming = GeneratedInterfaceNamingResolver.Resolve(symbol, attribute);
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

    private static KeyExpression? ExtractKey(AttributeData attribute)
    {
        var ctor = attribute.AttributeConstructor;
        if (ctor is { Parameters.Length: > 0 })
        {
            var keyParameterIndex = FindKeyParameterIndex(ctor);
            if (keyParameterIndex >= 0 && attribute.ConstructorArguments.Length > keyParameterIndex)
            {
                var keyArg = attribute.ConstructorArguments[keyParameterIndex];
                return ExtractKeyValue(keyArg);
            }
        }

        var keyNamedArg = attribute.NamedArguments.FirstOrDefault(na => na.Key == "Key");
        return ExtractKeyValue(keyNamedArg.Value);
    }

    private static KeyExpression? ExtractKeyValue(TypedConstant keyConstant)
    {
        if (keyConstant.IsNull)
        {
            return null;
        }

        if (keyConstant.Kind == TypedConstantKind.Type && keyConstant.Value is ITypeSymbol typeSymbol)
        {
            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return new KeyExpression($"typeof({AddGlobalAlias(typeName)})");
        }

        if (keyConstant.Type is { TypeKind: TypeKind.Enum } enumType)
        {
            var literal = ResolveEnumLiteral(enumType, keyConstant.Value);
            if (literal is not null)
            {
                return literal;
            }
        }

        return FormatPrimitiveLiteral(keyConstant);
    }

    private static KeyExpression? ResolveEnumLiteral(ITypeSymbol enumType, object? value)
    {
        if (value is null)
        {
            return null;
        }

        foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.ConstantValue is null)
            {
                continue;
            }

            if (Equals(member.ConstantValue, value))
            {
                var typeName = enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return new KeyExpression($"{AddGlobalAlias(typeName)}.{member.Name}");
            }
        }

        return null;
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

    private static KeyExpression? FormatPrimitiveLiteral(TypedConstant keyConstant)
    {
        if (keyConstant.Kind != TypedConstantKind.Primitive)
        {
            return keyConstant.Value is null
                ? null
                : new KeyExpression(SymbolDisplay.FormatLiteral(keyConstant.Value.ToString(), quote: true));
        }

        if (keyConstant.Type is null)
        {
            return keyConstant.Value is null
                ? null
                : new KeyExpression(SymbolDisplay.FormatLiteral(keyConstant.Value.ToString(), quote: true));
        }

        return keyConstant.Type.SpecialType switch
        {
            SpecialType.System_String => new KeyExpression(SymbolDisplay.FormatLiteral((string)keyConstant.Value!, quote: true)),
            SpecialType.System_Char => new KeyExpression(SymbolDisplay.FormatLiteral((char)keyConstant.Value!, quote: true)),
            SpecialType.System_Boolean => new KeyExpression((bool)keyConstant.Value! ? "true" : "false"),
            SpecialType.System_Int32 => new KeyExpression(((int)keyConstant.Value!).ToString(CultureInfo.InvariantCulture)),
            SpecialType.System_Int64 => new KeyExpression($"{((long)keyConstant.Value!).ToString(CultureInfo.InvariantCulture)}L"),
            SpecialType.System_UInt32 => new KeyExpression($"{((uint)keyConstant.Value!).ToString(CultureInfo.InvariantCulture)}U"),
            SpecialType.System_UInt64 => new KeyExpression($"{((ulong)keyConstant.Value!).ToString(CultureInfo.InvariantCulture)}UL"),
            SpecialType.System_Int16 => new KeyExpression($"(short){((short)keyConstant.Value!).ToString(CultureInfo.InvariantCulture)}"),
            SpecialType.System_UInt16 => new KeyExpression($"(ushort){((ushort)keyConstant.Value!).ToString(CultureInfo.InvariantCulture)}"),
            SpecialType.System_Byte => new KeyExpression($"(byte){((byte)keyConstant.Value!).ToString(CultureInfo.InvariantCulture)}"),
            SpecialType.System_SByte => new KeyExpression($"(sbyte){((sbyte)keyConstant.Value!).ToString(CultureInfo.InvariantCulture)}"),
            SpecialType.System_Single => new KeyExpression($"{((float)keyConstant.Value!).ToString("R", CultureInfo.InvariantCulture)}f"),
            SpecialType.System_Double => new KeyExpression($"{((double)keyConstant.Value!).ToString("R", CultureInfo.InvariantCulture)}d"),
            SpecialType.System_Decimal => new KeyExpression($"{((decimal)keyConstant.Value!).ToString(CultureInfo.InvariantCulture)}m"),
            _ => keyConstant.Value is null
                ? null
                : new KeyExpression(SymbolDisplay.FormatLiteral(keyConstant.Value.ToString(), quote: true))
        };
    }
}

internal sealed record KeyExpression(string Code);
