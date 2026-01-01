using System;

namespace AttributedDI;

/// <summary>
/// Assembly-level attribute that customizes the generated service registration module.
/// Specifies the module class name, extension method name, and namespace.
/// </summary>
/// <example>
/// <code>
/// // Customize module, method, and namespace
/// [assembly: GeneratedModule(moduleName: "MyFeatureModule", methodName: "AddMyFeature", moduleNamespace: "My.Company.Features")]
/// 
/// // Customize only method name (module name derived from assembly)
/// [assembly: GeneratedModule(methodName: "AddMyFeature")]
/// 
/// // Customize only module name (method name uses the default derived from assembly name)
/// [assembly: GeneratedModule(moduleName: "MyFeatureModule")]
/// 
/// // Customize only namespace (module and method derived from assembly)
/// [assembly: GeneratedModule(moduleNamespace: "My.Company.Generated")]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class GeneratedModuleAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratedModuleAttribute"/> class.
    /// </summary>
    /// <param name="moduleName">The name for the generated module class. If null, derived from assembly name.</param>
    /// <param name="methodName">The name for the registration extension method. If null, uses "Add" + module name.</param>
    /// <param name="moduleNamespace">The namespace for the generated module and extension class. If null, derived from assembly name.</param>
    /// <exception cref="ArgumentException">Thrown when a provided value is empty or whitespace.</exception>
    public GeneratedModuleAttribute(string? moduleName = null, string? methodName = null, string? moduleNamespace = null)
    {
        if (moduleName is not null && string.IsNullOrWhiteSpace(moduleName))
        {
            throw new ArgumentException("Module name cannot be empty or whitespace.", nameof(moduleName));
        }

        if (methodName is not null && string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Method name cannot be empty or whitespace.", nameof(methodName));
        }

        if (moduleNamespace is not null && string.IsNullOrWhiteSpace(moduleNamespace))
        {
            throw new ArgumentException("Namespace cannot be empty or whitespace.", nameof(moduleNamespace));
        }

        ModuleName = moduleName;
        MethodName = methodName;
        Namespace = moduleNamespace;
    }

    /// <summary>
    /// Gets the name for the generated module class, or null to use the default derived from assembly name.
    /// </summary>
    public string? ModuleName { get; }

    /// <summary>
    /// Gets the name for the registration extension method, or null to use the default based on module name.
    /// </summary>
    public string? MethodName { get; }

    /// <summary>
    /// Gets the namespace for the generated module and extension class, or null to use the derived default.
    /// </summary>
    public string? Namespace { get; }
}