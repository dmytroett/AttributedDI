namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;

public static class TypedDelegatesServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        return new TypedDelegatesServiceProvider(TypedDelegatesGeneratedServiceExports.Exports);
    }
}
