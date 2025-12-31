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
    public const string RegisterAsAttribute = "RegisterAsAttribute"; // Generic attribute, use Name property
    public const string RegisterAsImplementedInterfacesAttribute = "AttributedDI.RegisterAsImplementedInterfacesAttribute";

    // Assembly-level attributes
    public const string GeneratedModuleNameAttribute = "AttributedDI.GeneratedModuleNameAttribute";
}