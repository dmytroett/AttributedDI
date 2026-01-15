using System.Text;

namespace AttributedDI.SourceGenerator;

internal static class GeneratedCodeHelper
{
    public static string GeneratorName { get; } = typeof(GeneratedCodeHelper).Assembly.GetName().Name ?? "AttributedDI.SourceGenerator";

    public static string GeneratorVersion { get; } = typeof(GeneratedCodeHelper).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";

    public static void AppendGeneratedCodeAttribute(StringBuilder sb, int indentLevel)
    {
        sb.Append(' ', indentLevel * 4)
            .Append("[global::System.CodeDom.Compiler.GeneratedCode(\"")
            .Append(GeneratorName)
            .Append("\", \"")
            .Append(GeneratorVersion)
            .AppendLine("\")]");
    }
}