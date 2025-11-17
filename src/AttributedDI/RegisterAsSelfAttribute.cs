using System;

namespace AttributedDI
{
    /// <summary>
    /// Marks the type to be registered in <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> as itself.
    /// </summary>
    /// <remarks>
    /// Use <see cref="TransientAttribute"/>, <see cref="ScopedAttribute"/>, or <see cref="SingletonAttribute"/> 
    /// to specify the lifetime. If no lifetime attribute is present, transient is used by default.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class RegisterAsSelfAttribute : Attribute
    {
    }
}