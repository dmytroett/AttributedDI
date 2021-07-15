using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace AttributedDI
{
    public static class RegistrationServiceCollectionExtensions
    {
        private static readonly AssemblyScanner AssemblyScanner = new();

        public static IServiceCollection AddServicesFromAssemblyContainingType<T>(this IServiceCollection services)
        {
            return AddServicesFromAssembly(services, typeof(T).Assembly);
        }

        public static IServiceCollection AddServicesFromAssemblyContainingType(this IServiceCollection services, Type type)
        {
            return AddServicesFromAssembly(services, type.Assembly);
        }

        public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var scanResults = AssemblyScanner.Scan(assembly);

            foreach (var scanResult in scanResults)
            {
                scanResult.RegisterAttribute.PerformRegistration(services, scanResult.Service);
            }

            return services;
        }
    }
}