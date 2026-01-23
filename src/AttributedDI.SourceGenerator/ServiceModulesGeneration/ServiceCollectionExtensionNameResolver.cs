using System.Linq;

namespace AttributedDI.SourceGenerator.ServiceModulesGeneration;

/// <summary>
/// Resolves generated service-collection extension names from assembly names and custom naming information.
/// </summary>
internal static class ServiceCollectionExtensionNameResolver
{
    /// <summary>
    /// Resolves the generated extension class name, method name, and namespace for an assembly.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly (e.g., "CompanyName.TeamName.ProjectName.API").</param>
    /// <param name="customNameInfo">Custom extension name information from attributes, or null to use defaults.</param>
    /// <returns>The resolved extension class name, method name, and namespace.</returns>
    public static ResolvedExtensionNames Resolve(string assemblyName, CustomExtensionNameInfo? customNameInfo)
    {
        string defaultClassName = SanitizeIdentifier($"{assemblyName}ServiceCollectionExtensions");
        string defaultNamespace = SanitizeNamespace(assemblyName);

        (string className, string namespaceName) = ResolveExtensionType(
            customNameInfo?.ExtensionClassName,
            customNameInfo?.ExtensionNamespace,
            defaultClassName,
            defaultNamespace);

        string methodName = ResolveMethodName(assemblyName, customNameInfo, defaultMethodName: $"Add{assemblyName}");

        return new ResolvedExtensionNames(className, methodName, namespaceName);
    }

    private static (string ClassName, string NamespaceName) ResolveExtensionType(
        string? extensionClassName,
        string? extensionNamespace,
        string defaultClassName,
        string defaultNamespace)
    {
        if (string.IsNullOrWhiteSpace(extensionClassName))
        {
            string namespaceName = SanitizeNamespace(extensionNamespace ?? defaultNamespace);
            return (defaultClassName, namespaceName);
        }

        string normalized = extensionClassName!;
        const string globalPrefix = "global::";
        if (normalized.StartsWith(globalPrefix, System.StringComparison.Ordinal))
        {
            normalized = normalized.Substring(globalPrefix.Length);
        }

        int lastDot = normalized.LastIndexOf('.');
        if (lastDot >= 0)
        {
            string namespacePart = normalized.Substring(0, lastDot);
            string classPart = normalized.Substring(lastDot + 1);

            string className = SanitizeIdentifier(classPart);
            if (string.IsNullOrWhiteSpace(className))
            {
                className = defaultClassName;
            }

            string namespaceName = SanitizeNamespace(namespacePart);
            return (className, namespaceName);
        }

        string simpleClassName = SanitizeIdentifier(normalized);
        if (string.IsNullOrWhiteSpace(simpleClassName))
        {
            simpleClassName = defaultClassName;
        }

        string simpleNamespace = SanitizeNamespace(extensionNamespace ?? defaultNamespace);
        return (simpleClassName, simpleNamespace);
    }

    private static string ResolveMethodName(
        string assemblyName,
        CustomExtensionNameInfo? customNameInfo,
        string defaultMethodName)
    {
        string baseName = customNameInfo?.MethodName ?? defaultMethodName;
        string sanitized = SanitizeIdentifier(baseName);

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return SanitizeIdentifier($"Add{assemblyName}");
        }

        return sanitized;
    }

    private static string SanitizeIdentifier(string name)
    {
        // Remove invalid characters to create a valid C# identifier
        string sanitized = new([.. name.Where(c => char.IsLetterOrDigit(c) || c == '_')]);

        // Ensure identifier doesn't start with a digit
        if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
        {
            sanitized = "_" + sanitized;
        }

        return sanitized;
    }

    private static string SanitizeNamespace(string namespaceValue)
    {
        const string globalPrefix = "global::";
        if (namespaceValue.StartsWith(globalPrefix, System.StringComparison.Ordinal))
        {
            namespaceValue = namespaceValue.Substring(globalPrefix.Length);
        }

        string sanitized = new([.. namespaceValue.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '_')]);

        if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
        {
            sanitized = "_" + sanitized;
        }

        if (string.IsNullOrWhiteSpace(sanitized.Replace(".", string.Empty).Replace("_", string.Empty)))
        {
            return "AttributedDI";
        }

        return sanitized;
    }
}
