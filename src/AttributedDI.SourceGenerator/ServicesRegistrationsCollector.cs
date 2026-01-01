using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace AttributedDI.SourceGenerator;

/// <summary>
/// Collects attribute-based service registrations from the compilation.
/// </summary>
internal static class ServicesRegistrationsCollector
{
    /// <summary>
    /// Collects service registrations from the assembly.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <returns>An incremental values provider of registration information.</returns>
    public static IncrementalValuesProvider<TypeWithAttributesInfo> Collect(
        IncrementalGeneratorInitializationContext context)
    {
        var registerAsSelfTypes = CreateAttributeCollector(context, KnownAttributes.RegisterAsSelfAttribute, ExtractTypeInfo);
        var registerAsImplementedInterfacesTypes = CreateAttributeCollector(context, KnownAttributes.RegisterAsImplementedInterfacesAttribute, ExtractTypeInfo);
        var registerAsTypes = CreateAttributeCollector(context, KnownAttributes.RegisterAsAttribute, ExtractRegisterAsInfo);

        var transientLifetimeTypes = CreateAttributeCollector(context, KnownAttributes.TransientAttribute, ExtractLifetimeOnlyInfo);
        var scopedLifetimeTypes = CreateAttributeCollector(context, KnownAttributes.ScopedAttribute, ExtractLifetimeOnlyInfo);
        var singletonLifetimeTypes = CreateAttributeCollector(context, KnownAttributes.SingletonAttribute, ExtractLifetimeOnlyInfo);

        return AggregateProviders(
            registerAsSelfTypes,
            registerAsImplementedInterfacesTypes,
            registerAsTypes,
            transientLifetimeTypes,
            scopedLifetimeTypes,
            singletonLifetimeTypes);
    }

    private static IncrementalValuesProvider<TypeWithAttributesInfo> CreateAttributeCollector(
        IncrementalGeneratorInitializationContext context,
        string attributeMetadataName,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, TypeWithAttributesInfo?> transform)
    {
        return context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: attributeMetadataName,
                predicate: static (_, _) => true,
                transform: transform)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);
    }

    private static IncrementalValuesProvider<TypeWithAttributesInfo> AggregateProviders(
        params IncrementalValuesProvider<TypeWithAttributesInfo>[] providers)
    {
        if (providers.Length == 0)
        {
            throw new InvalidOperationException("No providers supplied.");
        }

        if (providers.Length == 1)
        {
            return providers[0];
        }

        var merged = providers[0].Collect();

        for (var i = 1; i < providers.Length; i++)
        {
            merged = merged
                .Combine(providers[i].Collect())
                .Select(static (pair, _) => pair.Left.AddRange(pair.Right));
        }

        return merged.SelectMany(static (items, _) => items);
    }

    /// <summary>
    /// Extracts type information from an attribute context.
    /// </summary>
    private static TypeWithAttributesInfo? ExtractTypeInfo(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        var symbol = (INamedTypeSymbol?)context.TargetSymbol;
        if (symbol is null)
            return null;

        var attribute = context.Attributes[0];
        var lifetime = ResolveLifetime(symbol);
        var attributeName = attribute.AttributeClass?.ToDisplayString();

        // Determine registration type based on attribute
        var registrationType = attributeName switch
        {
            var n when n == KnownAttributes.RegisterAsSelfAttribute => RegistrationType.RegisterAsSelf,
            var n when n == KnownAttributes.RegisterAsImplementedInterfacesAttribute => RegistrationType.RegisterAsImplementedInterfaces,
            _ => RegistrationType.RegisterAsSelf
        };

        var registrations = ImmutableArray.CreateBuilder<RegistrationInfo>();

        switch (registrationType)
        {
            case RegistrationType.RegisterAsSelf:
                registrations.Add(new RegistrationInfo(
                    FullyQualifiedTypeName: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Namespace: symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                    TypeName: symbol.Name,
                    RegistrationType: RegistrationType.RegisterAsSelf,
                    ServiceTypeFullName: null,
                    Lifetime: lifetime,
                    Key: ExtractKey(attribute)));
                break;

            case RegistrationType.RegisterAsImplementedInterfaces:
                ct.ThrowIfCancellationRequested();

                // Extract all interfaces except IDisposable
                var interfaces = symbol.AllInterfaces
                    .Where(static iface => !ImplementsDisposableContract(iface))
                    .Distinct(SymbolEqualityComparer.Default)
                    .ToList();

                foreach (var iface in interfaces)
                {
                    registrations.Add(new RegistrationInfo(
                        FullyQualifiedTypeName: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Namespace: symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                        TypeName: symbol.Name,
                        RegistrationType: RegistrationType.RegisterAsImplementedInterfaces,
                        ServiceTypeFullName: iface?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty,
                        Lifetime: lifetime,
                        Key: ExtractKey(attribute)));
                }

                // If type implements no interfaces (besides IDisposable), still register it as self
                if (registrations.Count == 0)
                {
                    registrations.Add(new RegistrationInfo(
                        FullyQualifiedTypeName: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Namespace: symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                        TypeName: symbol.Name,
                        RegistrationType: RegistrationType.RegisterAsImplementedInterfaces,
                        ServiceTypeFullName: null,
                        Lifetime: lifetime,
                        Key: ExtractKey(attribute)));
                }

                break;
        }

        return registrations.Count > 0
            ? new TypeWithAttributesInfo(registrations.ToImmutable())
            : null;
    }

    /// <summary>
    /// Checks if a type symbol implements IDisposable or IAsyncDisposable.
    /// </summary>
    private static bool ImplementsDisposableContract(ITypeSymbol interfaceSymbol)
    {
        return IsDisposableInterface(interfaceSymbol) || interfaceSymbol.AllInterfaces.Any(IsDisposableInterface);
    }

    /// <summary>
    /// Checks if a type symbol is IDisposable or IAsyncDisposable.
    /// </summary>
    private static bool IsDisposableInterface(ITypeSymbol interfaceSymbol)
    {
        if (interfaceSymbol.SpecialType == SpecialType.System_IDisposable)
        {
            return true;
        }

        return interfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.IAsyncDisposable";
    }

    /// <summary>
    /// Extracts RegisterAs attribute information from syntax context.
    /// </summary>
    private static TypeWithAttributesInfo? ExtractRegisterAsInfo(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
            return null;

        var registrations = ImmutableArray.CreateBuilder<RegistrationInfo>();
        var lifetime = ResolveLifetime(symbol);

        foreach (var registerAsAttribute in context.Attributes)
        {
            ct.ThrowIfCancellationRequested();

            if (registerAsAttribute.AttributeClass is not { TypeArguments.Length: > 0 } attrClass)
                continue;

            var serviceType = attrClass.TypeArguments[0];

            registrations.Add(new RegistrationInfo(
                FullyQualifiedTypeName: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Namespace: symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                TypeName: symbol.Name,
                RegistrationType: RegistrationType.RegisterAs,
                ServiceTypeFullName: serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Lifetime: lifetime,
                Key: ExtractKey(registerAsAttribute)));
        }

        return registrations.Count > 0
            ? new TypeWithAttributesInfo(registrations.ToImmutable())
            : null;
    }

    /// <summary>
    /// Extracts type information for types with only lifetime attributes.
    /// These are registered as self.
    /// </summary>
    private static TypeWithAttributesInfo? ExtractLifetimeOnlyInfo(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
            return null;

        if (HasRegistrationAttribute(symbol))
            return null;

        var lifetime = ResolveLifetime(symbol);

        var registration = new RegistrationInfo(
            FullyQualifiedTypeName: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Namespace: symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            TypeName: symbol.Name,
            RegistrationType: RegistrationType.RegisterAsSelf,
            ServiceTypeFullName: null,
            Lifetime: lifetime,
            Key: null);

        return new TypeWithAttributesInfo(
            ImmutableArray.Create(registration));
    }

    private static bool HasRegistrationAttribute(INamedTypeSymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            var displayName = attribute.AttributeClass?.ToDisplayString();

            if (displayName is null)
            {
                continue;
            }

            if (displayName == KnownAttributes.RegisterAsSelfAttribute ||
                displayName == KnownAttributes.RegisterAsImplementedInterfacesAttribute ||
                IsRegisterAsAttribute(attribute))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRegisterAsAttribute(AttributeData attribute)
    {
        if (attribute.AttributeClass is null)
        {
            return false;
        }

        return string.Equals(attribute.AttributeClass.Name, "RegisterAsAttribute", StringComparison.Ordinal) &&
               string.Equals(attribute.AttributeClass.ContainingNamespace?.ToDisplayString(), "AttributedDI", StringComparison.Ordinal);
    }

    /// <summary>
    /// Resolves the lifetime from lifetime attributes on a type.
    /// </summary>
    private static string ResolveLifetime(INamedTypeSymbol symbol)
    {
        var lifetimeAttr = symbol.GetAttributes()
            .FirstOrDefault(attr =>
            {
                var displayString = attr.AttributeClass?.ToDisplayString();
                return displayString == KnownAttributes.TransientAttribute ||
                       displayString == KnownAttributes.SingletonAttribute ||
                       displayString == KnownAttributes.ScopedAttribute;
            });

        if (lifetimeAttr is null)
            return "Transient";

        var attrName = lifetimeAttr.AttributeClass!.ToDisplayString();
        return attrName switch
        {
            var n when n == KnownAttributes.TransientAttribute => "Transient",
            var n when n == KnownAttributes.SingletonAttribute => "Singleton",
            var n when n == KnownAttributes.ScopedAttribute => "Scoped",
            _ => "Transient"
        };
    }

    /// <summary>
    /// Extracts the service key from an attribute's constructor or named arguments.
    /// </summary>
    private static object? ExtractKey(AttributeData attribute)
    {
        // Check constructor arguments first
        if (attribute.ConstructorArguments.Length > 0)
        {
            var keyArg = attribute.ConstructorArguments[0];
            return !keyArg.IsNull ? keyArg.Value : null;
        }

        // Check named arguments
        var keyNamedArg = attribute.NamedArguments.FirstOrDefault(na => na.Key == "Key");
        return !keyNamedArg.Value.IsNull ? keyNamedArg.Value.Value : null;
    }
}

internal enum RegistrationType
{
    RegisterAsSelf,
    RegisterAs,
    RegisterAsImplementedInterfaces
}

/// <summary>
/// Information about a single service registration extracted from attributes.
/// </summary>
/// <param name="FullyQualifiedTypeName">The fully qualified name of the implementation type.</param>
/// <param name="Namespace">The namespace of the implementation type.</param>
/// <param name="TypeName">The simple name of the implementation type.</param>
/// <param name="RegistrationType">The type of registration (self, as interface, etc.).</param>
/// <param name="ServiceTypeFullName">The fully qualified name of the service type (for RegisterAs), or null.</param>
/// <param name="Lifetime">The service lifetime (Transient, Scoped, or Singleton).</param>
/// <param name="Key">The service key, if this is a keyed registration.</param>
internal sealed record RegistrationInfo(
    string FullyQualifiedTypeName,
    string Namespace,
    string TypeName,
    RegistrationType RegistrationType,
    string? ServiceTypeFullName,
    string Lifetime,
    object? Key);

/// <summary>
/// Information about a type with service registration attributes.
/// </summary>
/// <param name="Registrations">The registrations declared on this type.</param>
internal sealed record TypeWithAttributesInfo(
    ImmutableArray<RegistrationInfo> Registrations);