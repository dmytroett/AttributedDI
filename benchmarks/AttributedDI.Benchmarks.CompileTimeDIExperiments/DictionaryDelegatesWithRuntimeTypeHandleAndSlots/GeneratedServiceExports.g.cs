using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandleAndSlots;

internal static class DictionaryDelegatesWithRuntimeTypeHandleAndSlotsGeneratedServiceExports
{
    private const int SingletonService1Slot = 0;
    private const int SingletonService2Slot = 1;
    private const int SingletonService3Slot = 2;

    private const int ScopedService3Slot = 0;
    private const int ScopedService2Slot = 1;
    private const int ScopedService1Slot = 2;

    public const int SingletonSlotCount = 3;
    public const int ScopedSlotCount = 3;

    public static IReadOnlyList<KeyValuePair<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>> Exports { get; } =
    [
        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(SingletonService1).TypeHandle,
            static context => ResolveSingletonService1(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(SingletonService2).TypeHandle,
            static context => ResolveSingletonService2(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(SingletonService3).TypeHandle,
            static context => ResolveSingletonService3(context)),

        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(ScopedService3).TypeHandle,
            static context => ResolveScopedService3(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(ScopedService2).TypeHandle,
            static context => ResolveScopedService2(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(ScopedService1).TypeHandle,
            static context => ResolveScopedService1(context)),

        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(TransientService4).TypeHandle,
            static context => ResolveTransientService4(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(TransientService3).TypeHandle,
            static context => ResolveTransientService3(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(TransientService2).TypeHandle,
            static context => ResolveTransientService2(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(TransientService1).TypeHandle,
            static context => ResolveTransientService1(context)),
    ];

    private static SingletonService1 ResolveSingletonService1(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateSingleton(
            SingletonService1Slot,
            static _ => new SingletonService1());
    }

    private static SingletonService2 ResolveSingletonService2(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateSingleton(
            SingletonService2Slot,
            static currentContext => new SingletonService2(ResolveSingletonService1(currentContext)));
    }

    private static SingletonService3 ResolveSingletonService3(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateSingleton(
            SingletonService3Slot,
            static currentContext => new SingletonService3(ResolveSingletonService2(currentContext)));
    }

    private static ScopedService3 ResolveScopedService3(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateScoped(
            ScopedService3Slot,
            static currentContext => new ScopedService3(ResolveSingletonService3(currentContext)));
    }

    private static ScopedService2 ResolveScopedService2(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateScoped(
            ScopedService2Slot,
            static currentContext => new ScopedService2(
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static ScopedService1 ResolveScopedService1(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateScoped(
            ScopedService1Slot,
            static currentContext => new ScopedService1(
                ResolveScopedService2(currentContext),
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static TransientService4 ResolveTransientService4(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return new TransientService4(ResolveScopedService3(context));
    }

    private static TransientService3 ResolveTransientService3(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return new TransientService3(
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService2 ResolveTransientService2(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return new TransientService2(
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService1 ResolveTransientService1(
        DictionaryDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return new TransientService1(
            ResolveTransientService2(context),
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }
}
