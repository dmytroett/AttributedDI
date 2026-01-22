using Microsoft.CodeAnalysis.CSharp;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

internal static class GeneratedOutputFormatter
{
    public static string FormatGeneratedTrees(CSharpCompilation compilation, int originalTreeCount)
    {
        return compilation.SyntaxTrees
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