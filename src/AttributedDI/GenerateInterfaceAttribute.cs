using System;

namespace AttributedDI;

/// <summary>
/// Marks a type to generate an interface that mirrors its public members.
/// </summary>
/// <remarks>
/// When no interface name is supplied, the generated interface name defaults to <c>I{TypeName}</c>
/// in the same namespace as the target type. Use the overloads to override the generated interface name and namespace.
/// If you also want the type to be registered against the generated interface, use
/// <see cref="RegisterAsGeneratedInterfaceAttribute"/> instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class GenerateInterfaceAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateInterfaceAttribute"/> class.
    /// </summary>
    /// <param name="interfaceName">Optional interface name to generate. Can be fully qualified.</param>
    /// <param name="interfaceNamespace">Optional namespace for the generated interface.</param>
    public GenerateInterfaceAttribute(string? interfaceName = null, string? interfaceNamespace = null)
    {
        InterfaceName = interfaceName;
        InterfaceNamespace = interfaceNamespace;
    }

    /// <summary>
    /// Gets the interface name to generate. Can be fully qualified.
    /// </summary>
    public string? InterfaceName { get; }

    /// <summary>
    /// Gets the namespace for the generated interface.
    /// </summary>
    public string? InterfaceNamespace { get; }
}