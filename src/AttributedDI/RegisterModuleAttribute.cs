using System;

namespace AttributedDI;

/// <summary>
/// Marks a class implementing <see cref="IServiceModule"/> for automatic service registration.
/// The module's <see cref="IServiceModule.ConfigureServices"/> method will be called during registration.
/// </summary>
/// <example>
/// <code>
/// [RegisterModule]
/// public class MyServiceModule : IServiceModule
/// {
///     public void ConfigureServices(IServiceCollection services)
///     {
///         services.AddLogging();
///         // Additional service registrations
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RegisterModuleAttribute : Attribute
{
}