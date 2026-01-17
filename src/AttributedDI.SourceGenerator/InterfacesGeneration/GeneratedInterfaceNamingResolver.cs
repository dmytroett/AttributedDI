using Microsoft.CodeAnalysis;
using System;

namespace AttributedDI.SourceGenerator.InterfacesGeneration;

internal static class GeneratedInterfaceNamingResolver
{
    internal static GeneratedInterfaceNaming Resolve(INamedTypeSymbol typeSymbol, AttributeData attribute)
    {
        var interfaceNameArgument = GetOptionalStringArgument(attribute, position: 0, name: "InterfaceName");
        var interfaceNamespaceArgument = GetOptionalStringArgument(attribute, position: 1, name: "InterfaceNamespace");

        var defaultInterfaceName = $"I{typeSymbol.Name}";
        var defaultNamespace = NormalizeNamespace(typeSymbol.ContainingNamespace?.ToDisplayString());

        if (!string.IsNullOrWhiteSpace(interfaceNamespaceArgument))
        {
            var parsed = !string.IsNullOrWhiteSpace(interfaceNameArgument)
                ? ParseInterfaceName(interfaceNameArgument!)
                : (Name: defaultInterfaceName, Namespace: string.Empty);

            return new GeneratedInterfaceNaming(
                parsed.Name,
                NormalizeNamespace(interfaceNamespaceArgument));
        }

        if (!string.IsNullOrWhiteSpace(interfaceNameArgument))
        {
            var parsed = ParseInterfaceName(interfaceNameArgument!);
            var resolvedNamespace = string.IsNullOrEmpty(parsed.Namespace)
                ? defaultNamespace
                : NormalizeNamespace(parsed.Namespace);

            return new GeneratedInterfaceNaming(parsed.Name, resolvedNamespace);
        }

        return new GeneratedInterfaceNaming(defaultInterfaceName, defaultNamespace);
    }

    private static string? GetOptionalStringArgument(AttributeData attribute, int position, string name)
    {
        if (attribute.ConstructorArguments.Length > position)
        {
            var ctorArg = attribute.ConstructorArguments[position];
            if (ctorArg.Value is string fromCtor && !string.IsNullOrWhiteSpace(fromCtor))
            {
                return fromCtor;
            }
        }

        foreach (var namedArgument in attribute.NamedArguments)
        {
            var key = namedArgument.Key;
            var typedConstant = namedArgument.Value;

            if (!string.Equals(key, name, StringComparison.Ordinal))
            {
                continue;
            }

            if (typedConstant.Value is string fromNamed && !string.IsNullOrWhiteSpace(fromNamed))
            {
                return fromNamed;
            }
        }

        return null;
    }

    private static (string Name, string Namespace) ParseInterfaceName(string rawName)
    {
        var nameToParse = rawName.StartsWith("global::", StringComparison.Ordinal)
            ? rawName.Substring("global::".Length)
            : rawName;

        var lastDot = nameToParse.LastIndexOf('.');
        if (lastDot < 0)
        {
            return (nameToParse, string.Empty);
        }

        var parsedNamespace = nameToParse[..lastDot];
        var parsedName = nameToParse[(lastDot + 1)..];
        return (parsedName, parsedNamespace);
    }

    private static string NormalizeNamespace(string? namespaceValue)
    {
        if (namespaceValue is null)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(namespaceValue))
        {
            return string.Empty;
        }

        if (string.Equals(namespaceValue, "<global namespace>", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        const string prefix = "global::";
        return namespaceValue.StartsWith(prefix, StringComparison.Ordinal)
            ? namespaceValue.Substring(prefix.Length)
            : namespaceValue;
    }
}

internal sealed record GeneratedInterfaceNaming(string InterfaceName, string InterfaceNamespace)
{
    internal string FullyQualifiedName => string.IsNullOrWhiteSpace(InterfaceNamespace)
        ? InterfaceName
        : $"{InterfaceNamespace}.{InterfaceName}";
}