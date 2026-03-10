namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointersWithRuntimeTypeHandle;

public static class DictionaryFunctionPointersWithRuntimeTypeHandleServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        return new DictionaryFunctionPointersWithRuntimeTypeHandleServiceProvider(
            DictionaryFunctionPointersWithRuntimeTypeHandleGeneratedServiceExports.Exports);
    }
}
