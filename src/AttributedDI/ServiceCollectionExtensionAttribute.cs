using System;

namespace AttributedDI;

/// <summary>
/// Assembly-level attribute that customizes the generated service-collection extension class and method.
/// </summary>
/// <example>
/// <code>
/// // Customize class, method, and namespace
/// [assembly: ServiceCollectionExtension(
///     extensionClassName: "MyExtensions",
///     methodName: "RegisterServices",
///     extensionNamespace: "My.Company.Generated")]
///
/// // Fully qualified class name (namespace ignored)
/// [assembly: ServiceCollectionExtension(extensionClassName: "My.Company.Generated.MyExtensions")]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ServiceCollectionExtensionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceCollectionExtensionAttribute"/> class.
    /// </summary>
    /// <param name="extensionClassName">
    /// The name of the generated extension class. May be a fully qualified name (namespace + class).
    /// </param>
    /// <param name="methodName">
    /// The name of the generated extension method. Fully replaces the default.
    /// </param>
    /// <param name="extensionNamespace">
    /// The namespace for the generated extension class when <paramref name="extensionClassName"/> is not fully qualified.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when a provided value is empty or whitespace.</exception>
    public ServiceCollectionExtensionAttribute(
        string? extensionClassName = null,
        string? methodName = null,
        string? extensionNamespace = null)
    {
        if (extensionClassName is not null && string.IsNullOrWhiteSpace(extensionClassName))
        {
            throw new ArgumentException("Extension class name cannot be empty or whitespace.", nameof(extensionClassName));
        }

        if (methodName is not null && string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Method name cannot be empty or whitespace.", nameof(methodName));
        }

        if (extensionNamespace is not null && string.IsNullOrWhiteSpace(extensionNamespace))
        {
            throw new ArgumentException("Namespace cannot be empty or whitespace.", nameof(extensionNamespace));
        }

        ExtensionClassName = extensionClassName;
        MethodName = methodName;
        ExtensionNamespace = extensionNamespace;
    }

    /// <summary>
    /// Gets the name for the generated extension class, or null to use the default derived from assembly name.
    /// </summary>
    public string? ExtensionClassName { get; }

    /// <summary>
    /// Gets the name for the generated extension method, or null to use the default derived from assembly name.
    /// </summary>
    public string? MethodName { get; }

    /// <summary>
    /// Gets the namespace for the generated extension class, or null to use the default derived from assembly name.
    /// </summary>
    public string? ExtensionNamespace { get; }
}
