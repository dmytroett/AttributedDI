using System;

namespace AttributedDI;

/// <summary>
/// Marks the type to be registered in <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> 
/// as implementation type for all implemented interfaces.
/// </summary>
/// <remarks>
/// Use <see cref="TransientAttribute"/>, <see cref="ScopedAttribute"/>, or <see cref="SingletonAttribute"/> 
/// to specify the lifetime. If no lifetime attribute is present, transient is used by default.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class RegisterAsImplementedInterfacesAttribute : Attribute
{
    /// <summary>
    /// Gets the service key for keyed service registration.
    /// </summary>
    /// <remarks>
    /// When null, a non-keyed service registration is generated.
    /// When set, a keyed service registration is generated using AddKeyed{Lifetime} methods.
    /// </remarks>
    public object? Key { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterAsImplementedInterfacesAttribute"/> class for non-keyed registration.
    /// </summary>
    public RegisterAsImplementedInterfacesAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterAsImplementedInterfacesAttribute"/> class for keyed registration.
    /// </summary>
    /// <param name="key">The service key to use for keyed service registration.</param>
    public RegisterAsImplementedInterfacesAttribute(object key)
    {
        Key = key;
    }
}