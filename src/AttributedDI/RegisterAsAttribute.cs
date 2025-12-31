using System;

namespace AttributedDI;

/// <summary>
/// Marks the type to be registered in <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> 
/// as implementation type for the specified service type.
/// </summary>
/// <typeparam name="TService">The service type to register as.</typeparam>
/// <remarks>
/// Use <see cref="TransientAttribute"/>, <see cref="ScopedAttribute"/>, or <see cref="SingletonAttribute"/> 
/// to specify the lifetime. If no lifetime attribute is present, transient is used by default.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
public sealed class RegisterAsAttribute<TService> : Attribute
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
    /// Initializes a new instance of the <see cref="RegisterAsAttribute{TService}"/> class for non-keyed registration.
    /// </summary>
    public RegisterAsAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterAsAttribute{TService}"/> class for keyed registration.
    /// </summary>
    /// <param name="key">The service key to use for keyed service registration.</param>
    public RegisterAsAttribute(object key)
    {
        Key = key;
    }
}