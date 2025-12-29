using System;

namespace AttributedDI;

/// <summary>
/// Assembly-level attribute that specifies the name for the generated registration extension method.
/// When applied, generates an Add{MethodName}() extension method instead of Add{AssemblyName}().
/// </summary>
/// <example>
/// <code>
/// [assembly: RegistrationMethodName("MyFeature")]
/// </code>
/// This will generate an AddMyFeature() method that registers all services from this assembly.
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class RegistrationMethodNameAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationMethodNameAttribute"/> class.
    /// </summary>
    /// <param name="methodName">The method name used to generate the registration method name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="methodName"/> is empty or whitespace.</exception>
    public RegistrationMethodNameAttribute(string methodName)
    {
        ArgumentNullException.ThrowIfNull(methodName);
        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentException("Method name cannot be empty or whitespace.", nameof(methodName));

        MethodName = methodName;
    }

    /// <summary>
    /// Gets the method name used for the registration method.
    /// </summary>
    public string MethodName { get; }
}