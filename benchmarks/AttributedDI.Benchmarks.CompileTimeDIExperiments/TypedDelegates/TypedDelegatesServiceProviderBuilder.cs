namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;

public static class TypedDelegatesServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        return BuildTypedServiceProvider();
    }

    internal static TypedDelegatesServiceProvider BuildTypedServiceProvider()
    {
        return new TypedDelegatesServiceProvider(TypedDelegatesGeneratedServiceExports.Exports);
    }
}
