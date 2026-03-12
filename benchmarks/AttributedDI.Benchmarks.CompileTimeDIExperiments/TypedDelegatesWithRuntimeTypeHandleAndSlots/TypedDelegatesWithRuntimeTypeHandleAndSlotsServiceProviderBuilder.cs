namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegatesWithRuntimeTypeHandleAndSlots;

public static class TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        return BuildTypedServiceProvider();
    }

    internal static TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider BuildTypedServiceProvider()
    {
        return new TypedDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider(
            TypedDelegatesWithRuntimeTypeHandleAndSlotsGeneratedServiceExports.Exports,
            TypedDelegatesWithRuntimeTypeHandleAndSlotsGeneratedServiceExports.SingletonSlotCount,
            TypedDelegatesWithRuntimeTypeHandleAndSlotsGeneratedServiceExports.ScopedSlotCount);
    }
}
