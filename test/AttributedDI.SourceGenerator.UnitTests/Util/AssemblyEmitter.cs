using Microsoft.CodeAnalysis;
using System.Globalization;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

internal static class AssemblyEmitter
{
    public static PortableExecutableReference EmitReference(Compilation compilation, string? assemblyName)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        if (!emitResult.Success)
        {
            string errorMessages = string.Join(
                Environment.NewLine,
                emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => $"  {d.GetMessage(CultureInfo.InvariantCulture)}"));
            Assert.Fail($"Reference assembly ({assemblyName ?? "Unnamed"}) emit failed:{Environment.NewLine}{errorMessages}");
        }

        stream.Position = 0;
        return MetadataReference.CreateFromStream(stream);
    }
}