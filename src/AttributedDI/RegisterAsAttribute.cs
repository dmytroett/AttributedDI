using Microsoft.Extensions.DependencyInjection;
using System;
namespace AttributedDI
{
    /// <summary>
    /// Marks the type to be registered in <see cref="IServiceCollection"/> as implementation type for service specified in constructor.
    /// </summary>
    public sealed class RegisterAsAttribute : RegisterBase
    {
        /// <summary>
        /// Creates an instance of attribute.
        /// </summary>
        /// <param name="serviceType">Service type.</param>
        /// <param name="lifetime">Service instance lifetime.</param>
        public RegisterAsAttribute(Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            Lifetime = lifetime;
            ServiceType = serviceType;
        }
        /// <summary>
        /// Registration service type.
        /// </summary>
        public Type ServiceType { get; }
        /// <summary>
        /// Registration lifetime.
        /// </summary>
        public ServiceLifetime Lifetime { get; }
    }
}