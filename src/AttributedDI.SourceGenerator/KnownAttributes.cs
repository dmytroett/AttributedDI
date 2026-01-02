namespace AttributedDI.SourceGenerator;

/// <summary>
/// Centralized registry of all known AttributedDI attribute names.
/// </summary>
internal static class KnownAttributes
{
    // Lifetime attributes
    public const string TransientAttribute = "AttributedDI.TransientAttribute";
    public const string SingletonAttribute = "AttributedDI.SingletonAttribute";
    public const string ScopedAttribute = "AttributedDI.ScopedAttribute";

    // Registration attributes
    public const string RegisterAsSelfAttribute = "AttributedDI.RegisterAsSelfAttribute";
    public const string RegisterAsAttribute = "AttributedDI.RegisterAsAttribute`1"; // Generic attribute, includes arity
    public const string RegisterAsImplementedInterfacesAttribute = "AttributedDI.RegisterAsImplementedInterfacesAttribute";
    public const string RegisterAsGeneratedInterfaceAttribute = "AttributedDI.RegisterAsGeneratedInterfaceAttribute";

    // Interface generation attributes
    public const string GenerateInterfaceAttribute = "AttributedDI.GenerateInterfaceAttribute";

    // Assembly-level attributes
    public const string GeneratedModuleAttribute = "AttributedDI.GeneratedModuleAttribute";
}