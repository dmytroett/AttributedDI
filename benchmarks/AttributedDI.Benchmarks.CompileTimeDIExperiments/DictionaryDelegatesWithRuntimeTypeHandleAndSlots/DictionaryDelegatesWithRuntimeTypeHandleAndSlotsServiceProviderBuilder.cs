namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandleAndSlots;

public static class DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceProviderBuilder
{
    public static IServiceProvider BuildServiceProvider()
    {
        return new DictionaryDelegatesWithRuntimeTypeHandleAndSlotsServiceProvider(
            DictionaryDelegatesWithRuntimeTypeHandleAndSlotsGeneratedServiceExports.Exports,
            DictionaryDelegatesWithRuntimeTypeHandleAndSlotsGeneratedServiceExports.SingletonSlotCount,
            DictionaryDelegatesWithRuntimeTypeHandleAndSlotsGeneratedServiceExports.ScopedSlotCount);
    }
}
