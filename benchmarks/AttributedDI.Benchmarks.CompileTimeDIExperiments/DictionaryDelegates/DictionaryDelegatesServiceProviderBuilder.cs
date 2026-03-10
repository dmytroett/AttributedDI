namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegates;

public static class DictionaryDelegatesServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        return new DictionaryDelegatesServiceProvider(DictionaryDelegatesGeneratedServiceExports.Exports);
    }
}
