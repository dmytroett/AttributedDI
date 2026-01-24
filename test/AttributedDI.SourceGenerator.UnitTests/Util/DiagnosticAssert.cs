using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

internal static class DiagnosticAssert
{
    public static void ContainsConflictingExtensionNamespace(
        ImmutableArray<Diagnostic> diagnostics,
        string className,
        string @namespace)
    {
        var diagnostic = FindById(diagnostics, "ATTDI003");
        AssertContainsParts(
            diagnostic,
            "ServiceCollectionExtensionAttribute",
            className,
            @namespace);
    }

    public static void ContainsConflictingInterfaceNamespace(
        ImmutableArray<Diagnostic> diagnostics,
        string attributeName,
        string interfaceName,
        string @namespace)
    {
        var diagnostic = FindById(diagnostics, "ATTDI005");
        AssertContainsParts(
            diagnostic,
            attributeName,
            interfaceName,
            @namespace);
    }

    public static void ContainsInvalidInterfaceUsage(
        ImmutableArray<Diagnostic> diagnostics,
        string attributeName,
        string typeName,
        params string[] reasonFragments)
    {
        var diagnostic = FindById(diagnostics, "ATTDI004");
        var fragments = new List<string>(2 + reasonFragments.Length)
        {
            attributeName,
            typeName,
        };
        fragments.AddRange(reasonFragments);
        AssertContainsParts(diagnostic, fragments.ToArray());
    }

    public static void ContainsConflictingLifetime(ImmutableArray<Diagnostic> diagnostics, string typeName)
    {
        var diagnostic = FindById(diagnostics, "ATTDI001");
        AssertContainsParts(diagnostic, typeName);
    }

    public static void ContainsInvalidMsBuildProperty(ImmutableArray<Diagnostic> diagnostics, string propertyName, string value)
    {
        var diagnostic = FindById(diagnostics, "ATTDI002");
        AssertContainsParts(diagnostic, propertyName, value);
    }

    private static Diagnostic FindById(ImmutableArray<Diagnostic> diagnostics, string id)
    {
        foreach (var diagnostic in diagnostics)
        {
            if (string.Equals(diagnostic.Id, id, StringComparison.Ordinal))
            {
                return diagnostic;
            }
        }

        Assert.Fail($"Expected diagnostic '{id}' was not found.");
        return null!;
    }

    private static void AssertContainsParts(Diagnostic diagnostic, params string[] fragments)
    {
        var message = diagnostic.GetMessage();
        foreach (var fragment in fragments)
        {
            Assert.Contains(fragment, message, StringComparison.Ordinal);
        }
    }
}
