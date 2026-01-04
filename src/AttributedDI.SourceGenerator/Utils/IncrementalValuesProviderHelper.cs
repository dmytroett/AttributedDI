using Microsoft.CodeAnalysis;
using System;

namespace AttributedDI.SourceGenerator.Utils;

public static class IncrementalValuesProviderHelper
{
    public static IncrementalValuesProvider<T> AggregateIncrementalProviders<T>(params IncrementalValuesProvider<T>[] providers)
    {
        if (providers.Length == 0)
        {
            throw new InvalidOperationException("No providers supplied.");
        }

        if (providers.Length == 1)
        {
            return providers[0];
        }

        var merged = providers[0].Collect();

        for (var i = 1; i < providers.Length; i++)
        {
            merged = merged
                .Combine(providers[i].Collect())
                .Select(static (pair, _) => pair.Left.AddRange(pair.Right));
        }

        return merged.SelectMany(static (items, _) => items);
    }
}