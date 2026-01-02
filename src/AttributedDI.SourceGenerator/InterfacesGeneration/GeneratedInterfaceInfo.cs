using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.InterfacesGeneration;

/// <summary>
/// Value object describing an interface to be generated.
/// </summary>
/// <param name="InterfaceName">The interface name without namespace.</param>
/// <param name="InterfaceNamespace">The namespace for the generated interface.</param>
/// <param name="Accessibility">The accessibility keyword (public/internal) to apply.</param>
/// <param name="MemberSignatures">Members to emit into the generated interface.</param>
internal sealed record GeneratedInterfaceInfo(
    string InterfaceName,
    string InterfaceNamespace,
    string Accessibility,
    ImmutableArray<string> MemberSignatures)
{
    internal string FullyQualifiedName => string.IsNullOrWhiteSpace(InterfaceNamespace)
        ? InterfaceName
        : $"{InterfaceNamespace}.{InterfaceName}";
}