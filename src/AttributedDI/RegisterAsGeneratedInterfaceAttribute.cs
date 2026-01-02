using System;

namespace AttributedDI;

/// <summary>
/// Marks a type to be registered against a generated interface in <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
/// </summary>
/// <remarks>
/// <para>Generation contract (supported):</para>
/// <list type="bullet">
/// <item><description>Instance members only; no static members or operators are emitted.</description></item>
/// <item><description>Public methods, properties, events, and indexers declared on the type.</description></item>
/// <item><description>Declared members only; inherited members are ignored.</description></item>
/// <item><description>Generic type parameters and constraints are preserved, including nullability annotations.</description></item>
/// </list>
/// <para>Not generated (ignored):</para>
/// <list type="bullet">
/// <item><description>Nested types.</description></item>
/// <item><description>Explicit interface implementations.</description></item>
/// <item><description>Non-public members.</description></item>
/// <item><description>Static members and operators.</description></item>
/// <item><description><c>ref</c> returns or <c>ref</c>/<c>in</c>/<c>out</c> parameters.</description></item>
/// <item><description>Members excluded via conditional compilation.</description></item>
/// </list>
/// <para>
/// When no name/namespace is provided, the generator uses <c>I{TypeName}</c> in the containing namespace.
/// Use this attribute when you want interface generation and automatic registration; if you only want the interface
/// without registration, prefer <see cref="GenerateInterfaceAttribute"/>.
/// </para>
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