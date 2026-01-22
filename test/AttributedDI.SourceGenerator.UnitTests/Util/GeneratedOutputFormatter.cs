using System.Globalization;
using System.Text;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

internal static class GeneratedCodeExtractor
{
    public static string ExtractGeneratedCode(CompilationResult compilationResult)
    {
        var originalTreeCount = compilationResult.OriginalCompilation.SyntaxTrees.Length;
        return compilationResult.UpdatedCompilation.SyntaxTrees
            .Skip(originalTreeCount)
            .Aggregate(new StringBuilder(), (sb, tree) =>
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"File name: {Path.GetFileName(tree.FilePath)}");
                sb.AppendLine(tree.ToString());

                return sb;
            })
            .ToString();
    }
}