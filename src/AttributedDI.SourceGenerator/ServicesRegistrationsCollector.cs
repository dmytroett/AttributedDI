using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
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
    public static IncrementalValuesProvider<RegistrationInfo> Collect(
        IncrementalGeneratorInitializationContext context)
    {
        var registrationCandidates = AggregateProviders(
            RegistrationCandidates.CollectRegisterAsSelf(context),
            RegistrationCandidates.CollectRegisterAsImplementedInterfaces(context),
            RegistrationCandidates.CollectRegisterAs(context),
            RegistrationCandidates.CollectRegisterAsGeneratedInterface(context));

        var lifetimeInfos = LifetimeInfoCollector.Collect(context);

        return registrationCandidates.Collect()
            .Combine(lifetimeInfos.Collect())
            .SelectMany(static (pair, _) => RegistrationAggregator.Aggregate(pair.Left, pair.Right));
    }

    private static IncrementalValuesProvider<T> AggregateProviders<T>(params IncrementalValuesProvider<T>[] providers)
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

    private static class RegistrationCandidates
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

            return ImmutableArray.Create(candidate);
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

            return ImmutableArray.Create(candidate);
        }
    }

    private static class LifetimeInfoCollector
    {
        internal static IncrementalValuesProvider<LifetimeInfo> Collect(IncrementalGeneratorInitializationContext context)
        {
            var transient = CreateLifetimeCollector(context, KnownAttributes.TransientAttribute, "Transient");
            var scoped = CreateLifetimeCollector(context, KnownAttributes.ScopedAttribute, "Scoped");
            var singleton = CreateLifetimeCollector(context, KnownAttributes.SingletonAttribute, "Singleton");

            return AggregateProviders(transient, scoped, singleton);
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

        private static LifetimeInfo? ExtractLifetimeInfo(
            GeneratorAttributeSyntaxContext context,
            string lifetime)
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

    private static class RegistrationAggregator
    {
        private const string DefaultLifetime = "Transient";

        internal static ImmutableArray<RegistrationInfo> Aggregate(
            ImmutableArray<RegistrationCandidate> candidates,
            ImmutableArray<LifetimeInfo> lifetimes)
        {
            var lifetimeByType = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var lifetimeInfo in lifetimes)
            {
                if (!lifetimeByType.ContainsKey(lifetimeInfo.FullyQualifiedTypeName))
                {
                    lifetimeByType.Add(lifetimeInfo.FullyQualifiedTypeName, lifetimeInfo.Lifetime);
                }
            }

            var registrationsBuilder = ImmutableArray.CreateBuilder<RegistrationInfo>();
            var seenRegistrations = new HashSet<RegistrationInfo>();
            var implementationsWithRegistrations = new HashSet<string>(StringComparer.Ordinal);

            foreach (var candidate in candidates)
            {
                var lifetime = lifetimeByType.TryGetValue(candidate.FullyQualifiedTypeName, out var lifetimeValue)
                    ? lifetimeValue
                    : DefaultLifetime;

                var registration = new RegistrationInfo(
                    candidate.FullyQualifiedTypeName,
                    candidate.ServiceTypeFullName,
                    lifetime,
                    candidate.Key);

                if (seenRegistrations.Add(registration))
                {
                    registrationsBuilder.Add(registration);
                }

                implementationsWithRegistrations.Add(candidate.FullyQualifiedTypeName);
            }

            foreach (var lifetimeInfo in lifetimes)
            {
                if (implementationsWithRegistrations.Contains(lifetimeInfo.FullyQualifiedTypeName))
                {
                    continue;
                }

                var registration = new RegistrationInfo(
                    lifetimeInfo.FullyQualifiedTypeName,
                    null,
                    lifetimeInfo.Lifetime,
                    null);

                if (seenRegistrations.Add(registration))
                {
                    registrationsBuilder.Add(registration);
                }
            }

            return registrationsBuilder.ToImmutable();
        }
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

/// <summary>
/// Information about a single service registration extracted from attributes.
/// </summary>
/// <param name="FullyQualifiedTypeName">The fully qualified name of the implementation type.</param>
/// <param name="ServiceTypeFullName">The fully qualified name of the service type, or null for self-registration.</param>
/// <param name="Lifetime">The service lifetime (Transient, Scoped, or Singleton).</param>
/// <param name="Key">The service key, if this is a keyed registration.</param>
internal sealed record RegistrationInfo(
    string FullyQualifiedTypeName,
    string? ServiceTypeFullName,
    string Lifetime,
    object? Key);

internal sealed record RegistrationCandidate(
    string FullyQualifiedTypeName,
    string? ServiceTypeFullName,
    object? Key);

internal sealed record LifetimeInfo(
    string FullyQualifiedTypeName,
    string Lifetime);