namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;

public static class DictionaryFunctionPointersServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        return new DictionaryFunctionPointersServiceProvider(
            DictionaryFunctionPointersGeneratedServiceExports.Exports);
    }
}
