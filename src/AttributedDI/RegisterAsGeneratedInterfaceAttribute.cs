using System;

namespace AttributedDI;

/// <summary>
/// Marks a type to be registered against a generated interface in <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
/// </summary>
/// <remarks>
/// The generated interface mirrors the public members of the target type. When no name/namespace is provided, the
/// generator uses <c>I{TypeName}</c> in the containing namespace. Use this attribute when you want interface
/// generation and automatic registration; if you only want the interface without registration, prefer
/// <see cref="GenerateInterfaceAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
public sealed class RegisterAsGeneratedInterfaceAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterAsGeneratedInterfaceAttribute"/> class.
    /// </summary>
    /// <param name="interfaceName">Optional interface name to generate. Can be fully qualified.</param>
    /// <param name="interfaceNamespace">Optional namespace for the generated interface.</param>
    /// <param name="key">Optional service key for keyed registrations.</param>
    public RegisterAsGeneratedInterfaceAttribute(string? interfaceName = null, string? interfaceNamespace = null, object? key = null)
    {
        InterfaceName = interfaceName;
        InterfaceNamespace = interfaceNamespace;
        Key = key;
    }

    /// <summary>
    /// Gets the interface name to generate. Can be fully qualified.
    /// </summary>
    public string? InterfaceName { get; }

    /// <summary>
    /// Gets the namespace for the generated interface.
    /// </summary>
    public string? InterfaceNamespace { get; }

    /// <summary>
    /// Gets the service key for keyed registration.
    /// </summary>
    public object? Key { get; }
}