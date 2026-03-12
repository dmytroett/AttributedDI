using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegatesWithRuntimeTypeHandleAndSlots;

internal static class TypedDelegatesWithRuntimeTypeHandleAndSlotsGeneratedServiceExports
{
    private const int SingletonService1Slot = 0;
    private const int SingletonService2Slot = 1;
    private const int SingletonService3Slot = 2;

    private const int ScopedService3Slot = 0;
    private const int ScopedService2Slot = 1;
    private const int ScopedService1Slot = 2;

    public const int SingletonSlotCount = 3;
    public const int ScopedSlotCount = 3;

    static TypedDelegatesWithRuntimeTypeHandleAndSlotsGeneratedServiceExports()
    {
        InitializeTypedResolvers();
    }

    public static IReadOnlyList<KeyValuePair<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>> Exports { get; } =
    [
        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(SingletonService1).TypeHandle,
            static context => ResolveSingletonService1(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(SingletonService2).TypeHandle,
            static context => ResolveSingletonService2(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(SingletonService3).TypeHandle,
            static context => ResolveSingletonService3(context)),

        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(ScopedService3).TypeHandle,
            static context => ResolveScopedService3(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(ScopedService2).TypeHandle,
            static context => ResolveScopedService2(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(ScopedService1).TypeHandle,
            static context => ResolveScopedService1(context)),

        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(TransientService4).TypeHandle,
            static context => ResolveTransientService4(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(TransientService3).TypeHandle,
            static context => ResolveTransientService3(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(TransientService2).TypeHandle,
            static context => ResolveTransientService2(context)),
        KeyValuePair.Create<RuntimeTypeHandle, Func<TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext, object>>(
            typeof(TransientService1).TypeHandle,
            static context => ResolveTransientService1(context)),
    ];

    private static void InitializeTypedResolvers()
    {
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<SingletonService1>.Resolve =
            static (_, context) => ResolveSingletonService1(context);
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<SingletonService2>.Resolve =
            static (_, context) => ResolveSingletonService2(context);
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<SingletonService3>.Resolve =
            static (_, context) => ResolveSingletonService3(context);
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<ScopedService3>.Resolve =
            static (_, context) => ResolveScopedService3(context);
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<ScopedService2>.Resolve =
            static (_, context) => ResolveScopedService2(context);
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<ScopedService1>.Resolve =
            static (_, context) => ResolveScopedService1(context);
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<TransientService4>.Resolve =
            static (_, context) => ResolveTransientService4(context);
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<TransientService3>.Resolve =
            static (_, context) => ResolveTransientService3(context);
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<TransientService2>.Resolve =
            static (_, context) => ResolveTransientService2(context);
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompiledResolver<TransientService1>.Resolve =
            static (_, context) => ResolveTransientService1(context);
    }

    private static SingletonService1 ResolveSingletonService1(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateSingleton(
            SingletonService1Slot,
            static _ => new SingletonService1());
    }

    private static SingletonService2 ResolveSingletonService2(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateSingleton(
            SingletonService2Slot,
            static currentContext => new SingletonService2(ResolveSingletonService1(currentContext)));
    }

    private static SingletonService3 ResolveSingletonService3(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateSingleton(
            SingletonService3Slot,
            static currentContext => new SingletonService3(ResolveSingletonService2(currentContext)));
    }

    private static ScopedService3 ResolveScopedService3(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateScoped(
            ScopedService3Slot,
            static currentContext => new ScopedService3(ResolveSingletonService3(currentContext)));
    }

    private static ScopedService2 ResolveScopedService2(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateScoped(
            ScopedService2Slot,
            static currentContext => new ScopedService2(
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static ScopedService1 ResolveScopedService1(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return context.GetOrCreateScoped(
            ScopedService1Slot,
            static currentContext => new ScopedService1(
                ResolveScopedService2(currentContext),
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static TransientService4 ResolveTransientService4(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return new TransientService4(ResolveScopedService3(context));
    }

    private static TransientService3 ResolveTransientService3(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return new TransientService3(
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService2 ResolveTransientService2(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return new TransientService2(
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService1 ResolveTransientService1(
        TypedDelegatesWithRuntimeTypeHandleAndSlotsCompileTimeProviderContext context)
    {
        return new TransientService1(
            ResolveTransientService2(context),
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }
}
