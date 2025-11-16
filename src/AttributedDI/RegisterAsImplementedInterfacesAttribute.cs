using Microsoft.Extensions.DependencyInjection;
using System;
namespace AttributedDI
{
    /// <summary>
    /// Marks the type to be registered in <see cref="IServiceCollection"/> as implementation type for all implemented interfaces.
    /// </summary>
    public sealed class RegisterAsImplementedInterfacesAttribute : RegisterBase
    {
        /// <summary>
        /// Creates an instance of the attribute.
        /// </summary>
        /// <param name="lifetime">Service instance lifetime.</param>
        public RegisterAsImplementedInterfacesAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            Lifetime = lifetime;
        }
        /// <summary>
        /// Registration lifetime.
        /// </summary>
        public ServiceLifetime Lifetime { get; }
    }
}