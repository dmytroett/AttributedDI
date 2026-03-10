namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandle;

public static class DictionaryDelegatesWithRuntimeTypeHandleServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        return new DictionaryDelegatesWithRuntimeTypeHandleServiceProvider(
            DictionaryDelegatesWithRuntimeTypeHandleGeneratedServiceExports.Exports);
    }
}
