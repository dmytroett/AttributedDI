namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandleAndRootSlots;

public static class DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        return new DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsServiceProvider(
            DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsGeneratedServiceExports.Exports,
            DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsGeneratedServiceExports.SingletonSlotCount,
            DictionaryDelegatesWithRuntimeTypeHandleAndRootSlotsGeneratedServiceExports.RootScopedSlotCount);
    }
}
