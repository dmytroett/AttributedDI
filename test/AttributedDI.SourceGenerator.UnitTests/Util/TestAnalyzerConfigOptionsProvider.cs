using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

internal sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private static readonly AnalyzerConfigOptions EmptyOptions = new TestAnalyzerConfigOptions(ImmutableDictionary<string, string>.Empty);
    private readonly AnalyzerConfigOptions _globalOptions;

    public TestAnalyzerConfigOptionsProvider(ImmutableDictionary<string, string> globalOptions)
    {
        _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
    }

    public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
    {
        return EmptyOptions;
    }

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
    {
        return EmptyOptions;
    }
}

internal sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly ImmutableDictionary<string, string> _options;

    public TestAnalyzerConfigOptions(ImmutableDictionary<string, string> options)
    {
        _options = options;
    }

    public override bool TryGetValue(string key, out string value)
    {
        if (_options.TryGetValue(key, out var storedValue))
        {
            value = storedValue;
            return true;
        }

        value = string.Empty;
        return false;
    }
}