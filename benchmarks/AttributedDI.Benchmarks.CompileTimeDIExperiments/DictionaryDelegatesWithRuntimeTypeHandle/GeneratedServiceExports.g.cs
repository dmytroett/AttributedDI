using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryDelegatesWithRuntimeTypeHandle;

internal static class DictionaryDelegatesWithRuntimeTypeHandleGeneratedServiceExports
{
    public static IReadOnlyList<DictionaryDelegatesWithRuntimeTypeHandleServiceExport> Exports { get; } =
    [
        new(typeof(SingletonService1).TypeHandle, static context => ResolveSingletonService1(context)),
        new(typeof(SingletonService2).TypeHandle, static context => ResolveSingletonService2(context)),
        new(typeof(SingletonService3).TypeHandle, static context => ResolveSingletonService3(context)),

        new(typeof(ScopedService3).TypeHandle, static context => ResolveScopedService3(context)),
        new(typeof(ScopedService2).TypeHandle, static context => ResolveScopedService2(context)),
        new(typeof(ScopedService1).TypeHandle, static context => ResolveScopedService1(context)),

        new(typeof(TransientService4).TypeHandle, static context => ResolveTransientService4(context)),
        new(typeof(TransientService3).TypeHandle, static context => ResolveTransientService3(context)),
        new(typeof(TransientService2).TypeHandle, static context => ResolveTransientService2(context)),
        new(typeof(TransientService1).TypeHandle, static context => ResolveTransientService1(context)),
    ];

    private static SingletonService1 ResolveSingletonService1(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService1).TypeHandle,
            context,
            static _ => new SingletonService1());
    }

    private static SingletonService2 ResolveSingletonService2(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService2).TypeHandle,
            context,
            static currentContext => new SingletonService2(ResolveSingletonService1(currentContext)));
    }

    private static SingletonService3 ResolveSingletonService3(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService3).TypeHandle,
            context,
            static currentContext => new SingletonService3(ResolveSingletonService2(currentContext)));
    }

    private static ScopedService3 ResolveScopedService3(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService3).TypeHandle,
            context,
            static currentContext => new ScopedService3(ResolveSingletonService3(currentContext)));
    }

    private static ScopedService2 ResolveScopedService2(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService2).TypeHandle,
            context,
            static currentContext => new ScopedService2(
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static ScopedService1 ResolveScopedService1(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService1).TypeHandle,
            context,
            static currentContext => new ScopedService1(
                ResolveScopedService2(currentContext),
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static TransientService4 ResolveTransientService4(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return new TransientService4(ResolveScopedService3(context));
    }

    private static TransientService3 ResolveTransientService3(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return new TransientService3(
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService2 ResolveTransientService2(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return new TransientService2(
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService1 ResolveTransientService1(
        DictionaryDelegatesWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return new TransientService1(
            ResolveTransientService2(context),
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }
}
