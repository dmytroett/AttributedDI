using System;

namespace AttributedDI;

/// <summary>
/// Assembly-level attribute that customizes the generated service registration module.
/// Specifies the name for both the generated module class and the extension method.
/// </summary>
/// <example>
/// <code>
/// // Customize both module and method names
/// [assembly: GeneratedModuleName(moduleName: "MyFeatureModule", methodName: "AddMyFeature")]
/// 
/// // This generates:
/// // public partial class MyFeatureModule : IServiceModule { ... }
/// // public static IServiceCollection AddMyFeature(this IServiceCollection services) { ... }
/// 
/// // Customize only method name (module name derived from assembly)
/// [assembly: GeneratedModuleName(methodName: "AddMyFeature")]
/// 
/// // Customize only module name (method name uses "Add" + module name)
/// [assembly: GeneratedModuleName(moduleName: "MyFeatureModule")]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class GeneratedModuleNameAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratedModuleNameAttribute"/> class.
    /// </summary>
    /// <param name="moduleName">The name for the generated module class. If null, derived from assembly name.</param>
    /// <param name="methodName">The name for the registration extension method. If null, uses "Add" + module name.</param>
    public GeneratedModuleNameAttribute(string? moduleName = null, string? methodName = null)
    {
        if (moduleName is not null && string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentException("Module name cannot be empty or whitespace.", nameof(moduleName));

        if (methodName is not null && string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentException("Method name cannot be empty or whitespace.", nameof(methodName));

        ModuleName = moduleName;
        MethodName = methodName;
    }

    /// <summary>
    /// Gets the name for the generated module class, or null to use the default derived from assembly name.
    /// </summary>
    public string? ModuleName { get; }

    /// <summary>
    /// Gets the name for the registration extension method, or null to use the default based on module name.
    /// </summary>
    public string? MethodName { get; }
}