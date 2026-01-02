using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AttributedDI.SourceGenerator.ServiceModulesGeneration;

/// <summary>
/// Collects attribute-based service registrations from the compilation.
/// </summary>
internal static class ServicesRegistrationsCollector
{
    private const string DefaultLifetime = "Transient";

    /// <summary>
    /// Collects service registrations from the assembly.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    /// <returns>An incremental values provider of registration information.</returns>
    public static IncrementalValuesProvider<RegistrationInfo> Collect(
        IncrementalGeneratorInitializationContext context)
    {
        var registrationCandidates = AggregateIncrementalProviders(
            RegistrationCandidatesCollector.CollectRegisterAsSelf(context),
            RegistrationCandidatesCollector.CollectRegisterAsImplementedInterfaces(context),
            RegistrationCandidatesCollector.CollectRegisterAs(context),
            RegistrationCandidatesCollector.CollectRegisterAsGeneratedInterface(context));

        var lifetimeInfos = LifetimeInfoCollector.Collect(context);

        return registrationCandidates.Collect()
            .Combine(lifetimeInfos.Collect())
            .SelectMany(static (pair, _) => Aggregate(pair.Left, pair.Right));
    }

    private static ImmutableArray<RegistrationInfo> Aggregate(
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