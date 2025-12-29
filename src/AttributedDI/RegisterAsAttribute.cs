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
}