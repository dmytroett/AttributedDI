using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace AttributedDI.SourceGenerator.UnitTests.Util;

internal sealed class FakeAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private static readonly AnalyzerConfigOptions EmptyOptions = new FakeAnalyzerConfigOptions(ImmutableDictionary<string, string>.Empty);
    private readonly AnalyzerConfigOptions _globalOptions;

    public FakeAnalyzerConfigOptionsProvider(ImmutableDictionary<string, string> globalOptions)
    {
        _globalOptions = new FakeAnalyzerConfigOptions(globalOptions);
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

internal sealed class FakeAnalyzerConfigOptions : AnalyzerConfigOptions
{
    private readonly ImmutableDictionary<string, string> _options;

    public FakeAnalyzerConfigOptions(ImmutableDictionary<string, string> options)
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