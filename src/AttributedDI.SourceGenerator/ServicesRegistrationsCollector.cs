using Microsoft.CodeAnalysis;
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
        // Collect RegisterAsSelf attributes
        var registerAsSelfTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName<TypeWithAttributesInfo?>(
                fullyQualifiedMetadataName: KnownAttributes.RegisterAsSelfAttribute,
                predicate: static (_, _) => true,
                transform: ExtractTypeInfo)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Collect RegisterAsImplementedInterfaces attributes
        var registerAsImplementedInterfacesTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName<TypeWithAttributesInfo?>(
                fullyQualifiedMetadataName: KnownAttributes.RegisterAsImplementedInterfacesAttribute,
                predicate: static (_, _) => true,
                transform: ExtractTypeInfo)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Collect RegisterAsAttribute - need to handle generic name differently
        var registerAsTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                {
                    if (node is not Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax typeDecl)
                        return false;

                    return typeDecl.AttributeLists
                        .SelectMany(al => al.Attributes)
                        .Any(attr =>
                        {
                            var name = attr.Name;
                            return name switch
                            {
                                Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax gns => gns.Identifier.Text == "RegisterAs",
                                Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax ins => ins.Identifier.Text == "RegisterAs",
                                _ => false
                            };
                        });
                },
                transform: ExtractRegisterAsInfo)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Collect types with lifetime attributes but no registration attributes
        // These should be registered as self
        var lifetimeOnlyTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                {
                    if (node is not Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax typeDecl)
                        return false;

                    // Check if it has a lifetime attribute
                    var hasLifetimeAttr = typeDecl.AttributeLists
                        .SelectMany(al => al.Attributes)
                        .Any(attr =>
                        {
                            var name = attr.Name;
                            var nameStr = name switch
                            {
                                Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax ins => ins.Identifier.Text,
                                _ => null
                            };
                            return nameStr == "Transient" || nameStr == "Singleton" || nameStr == "Scoped";
                        });

                    if (!hasLifetimeAttr)
                        return false;

                    // Check if it has NO registration attributes
                    var hasRegistrationAttr = typeDecl.AttributeLists
                        .SelectMany(al => al.Attributes)
                        .Any(attr =>
                        {
                            var name = attr.Name;
                            return name switch
                            {
                                Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax ins =>
                                    ins.Identifier.Text == "RegisterAsSelf" ||
                                    ins.Identifier.Text == "RegisterAsImplementedInterfaces",
                                Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax gns => gns.Identifier.Text == "RegisterAs",
                                _ => false
                            };
                        });

                    return !hasRegistrationAttr;
                },
                transform: ExtractLifetimeOnlyInfo)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Return all collected types from the four providers using Combine and SelectMany
        return registerAsSelfTypes
            .Collect()
            .Combine(registerAsImplementedInterfacesTypes.Collect())
            .Combine(registerAsTypes.Collect())
            .Combine(lifetimeOnlyTypes.Collect())
            .SelectMany(static (data, _) =>
            {
                var builder = ImmutableArray.CreateBuilder<TypeWithAttributesInfo>();
                builder.AddRange(data.Left.Left.Left);
                builder.AddRange(data.Left.Left.Right);
                builder.AddRange(data.Left.Right);
                builder.AddRange(data.Right);
                return builder.ToImmutable();
            });
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
    private static TypeWithAttributesInfo? ExtractRegisterAsInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        if (context.Node is not Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax typeDecl)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(typeDecl, cancellationToken: ct) is not INamedTypeSymbol symbol)
            return null;

        var registerAsAttributes = symbol.GetAttributes()
            .Where(attr => attr.AttributeClass?.Name == "RegisterAsAttribute")
            .ToList();

        if (registerAsAttributes.Count == 0)
            return null;

        var registrations = ImmutableArray.CreateBuilder<RegistrationInfo>();
        var lifetime = ResolveLifetime(symbol);

        foreach (var registerAsAttribute in registerAsAttributes)
        {
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
    private static TypeWithAttributesInfo? ExtractLifetimeOnlyInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        if (context.Node is not Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax typeDecl)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(typeDecl, cancellationToken: ct) is not INamedTypeSymbol symbol)
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