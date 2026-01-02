using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.InterfacesGeneration;

/// <summary>
/// Value object describing an interface to be generated.
/// </summary>
/// <param name="InterfaceName">The interface name without namespace.</param>
/// <param name="InterfaceNamespace">The namespace for the generated interface.</param>
/// <param name="Accessibility">The accessibility keyword (public/internal) to apply.</param>
/// <param name="MemberSignatures">Members to emit into the generated interface.</param>
/// <param name="ClassName">The implementing class name without namespace.</param>
/// <param name="ClassNamespace">The namespace for the implementing class.</param>
/// <param name="ClassTypeParameters">Type parameters of the implementing class (e.g., "&lt;T, U&gt;" or empty string).</param>
/// <param name="TypeParameterConstraints">Constraint clauses for the type parameters (e.g., "where T : class").</param>
internal sealed record GeneratedInterfaceInfo(
    string InterfaceName,
    string InterfaceNamespace,
    string Accessibility,
    ImmutableArray<string> MemberSignatures,
    string ClassName,
    string ClassNamespace,
    string ClassTypeParameters,
    int TypeParameterCount,
    string TypeParameterConstraints)
{
    internal string FullyQualifiedName => string.IsNullOrWhiteSpace(InterfaceNamespace)
        ? InterfaceName
        : $"{InterfaceNamespace}.{InterfaceName}";

    internal string UnboundInterfaceName => TypeParameterCount > 0
        ? $"{FullyQualifiedName}{BuildGenericAritySuffix(TypeParameterCount)}"
        : FullyQualifiedName;

    private static string BuildGenericAritySuffix(int typeParameterCount) => typeParameterCount switch
    {
        <= 0 => string.Empty,
        1 => "<>",
        _ => $"<{new string(',', typeParameterCount - 1)}>"
    };
}