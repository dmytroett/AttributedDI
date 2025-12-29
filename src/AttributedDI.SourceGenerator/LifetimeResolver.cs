using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace AttributedDI.SourceGenerator;

/// <summary>
/// Resolves service lifetime from attributes applied to a type.
/// </summary>
internal static class LifetimeResolver
{
    /// <summary>
    /// Resolves the lifetime of a service based on its attributes.
    /// </summary>
    /// <param name="attributes">The attributes to examine.</param>
    /// <returns>
    /// The lifetime string ("Transient", "Singleton", or "Scoped"), 
    /// or null if multiple lifetime attributes are found.
    /// </returns>
    public static string? ResolveLifetime(ImmutableArray<AttributeData> attributes)
    {
        // TODO: Add diagnostic reporting for multiple lifetime attributes
        var lifetimeAttributes = attributes
            .Where(attr => attr.AttributeClass != null &&
                          (attr.AttributeClass.ToDisplayString() == KnownAttributes.TransientAttribute ||
                           attr.AttributeClass.ToDisplayString() == KnownAttributes.SingletonAttribute ||
                           attr.AttributeClass.ToDisplayString() == KnownAttributes.ScopedAttribute))
            .ToList();

        // Multiple lifetime attributes found - invalid
        if (lifetimeAttributes.Count > 1)
        {
            return null;
        }

        // No lifetime attribute - default to Transient
        if (lifetimeAttributes.Count == 0)
        {
            return "Transient";
        }

        // Single lifetime attribute found
        var lifetimeAttr = lifetimeAttributes[0];
        var lifetimeTypeName = lifetimeAttr.AttributeClass!.ToDisplayString();

        if (lifetimeTypeName == KnownAttributes.TransientAttribute)
            return "Transient";
        if (lifetimeTypeName == KnownAttributes.SingletonAttribute)
            return "Singleton";
        if (lifetimeTypeName == KnownAttributes.ScopedAttribute)
            return "Scoped";

        return "Transient";
    }
}