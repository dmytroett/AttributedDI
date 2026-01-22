using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

internal static class CompilationAssertions
{
    public static void AssertCompiles(Compilation compilation, string stageName)
    {
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        if (errors.Count > 0)
        {
            string errorMessages = string.Join(
                Environment.NewLine,
                errors.Select(e => $"  {e.GetMessage(CultureInfo.InvariantCulture)}"));
            Assert.Fail($"{stageName} source code has compilation errors:{Environment.NewLine}{errorMessages}");
        }
    }
}