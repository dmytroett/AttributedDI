using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointersWithRuntimeTypeHandle;

internal static class DictionaryFunctionPointersWithRuntimeTypeHandleGeneratedServiceExports
{
    public static IReadOnlyList<DictionaryFunctionPointersWithRuntimeTypeHandleServiceExport> Exports { get; } =
    [
        new(typeof(SingletonService1).TypeHandle, GetFactoryPointer(nameof(ResolveSingletonService1Export))),
        new(typeof(SingletonService2).TypeHandle, GetFactoryPointer(nameof(ResolveSingletonService2Export))),
        new(typeof(SingletonService3).TypeHandle, GetFactoryPointer(nameof(ResolveSingletonService3Export))),

        new(typeof(ScopedService3).TypeHandle, GetFactoryPointer(nameof(ResolveScopedService3Export))),
        new(typeof(ScopedService2).TypeHandle, GetFactoryPointer(nameof(ResolveScopedService2Export))),
        new(typeof(ScopedService1).TypeHandle, GetFactoryPointer(nameof(ResolveScopedService1Export))),

        new(typeof(TransientService4).TypeHandle, GetFactoryPointer(nameof(ResolveTransientService4Export))),
        new(typeof(TransientService3).TypeHandle, GetFactoryPointer(nameof(ResolveTransientService3Export))),
        new(typeof(TransientService2).TypeHandle, GetFactoryPointer(nameof(ResolveTransientService2Export))),
        new(typeof(TransientService1).TypeHandle, GetFactoryPointer(nameof(ResolveTransientService1Export))),
    ];

    private static IntPtr GetFactoryPointer(string methodName)
    {
        return typeof(DictionaryFunctionPointersWithRuntimeTypeHandleGeneratedServiceExports)
            .GetMethod(
                methodName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MethodHandle
            .GetFunctionPointer();
    }

    private static object ResolveSingletonService1Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveSingletonService1(context);
    }

    private static object ResolveSingletonService2Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveSingletonService2(context);
    }

    private static object ResolveSingletonService3Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveSingletonService3(context);
    }

    private static object ResolveScopedService3Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveScopedService3(context);
    }

    private static object ResolveScopedService2Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveScopedService2(context);
    }

    private static object ResolveScopedService1Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveScopedService1(context);
    }

    private static object ResolveTransientService4Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveTransientService4(context);
    }

    private static object ResolveTransientService3Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveTransientService3(context);
    }

    private static object ResolveTransientService2Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveTransientService2(context);
    }

    private static object ResolveTransientService1Export(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return ResolveTransientService1(context);
    }

    private static SingletonService1 ResolveSingletonService1(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService1).TypeHandle,
            context,
            static _ => new SingletonService1());
    }

    private static SingletonService2 ResolveSingletonService2(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService2).TypeHandle,
            context,
            static currentContext => new SingletonService2(ResolveSingletonService1(currentContext)));
    }

    private static SingletonService3 ResolveSingletonService3(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService3).TypeHandle,
            context,
            static currentContext => new SingletonService3(ResolveSingletonService2(currentContext)));
    }

    private static ScopedService3 ResolveScopedService3(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService3).TypeHandle,
            context,
            static currentContext => new ScopedService3(ResolveSingletonService3(currentContext)));
    }

    private static ScopedService2 ResolveScopedService2(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService2).TypeHandle,
            context,
            static currentContext => new ScopedService2(
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static ScopedService1 ResolveScopedService1(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
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
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return new TransientService4(ResolveScopedService3(context));
    }

    private static TransientService3 ResolveTransientService3(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return new TransientService3(
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService2 ResolveTransientService2(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return new TransientService2(
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService1 ResolveTransientService1(
        DictionaryFunctionPointersWithRuntimeTypeHandleCompileTimeProviderContext context)
    {
        return new TransientService1(
            ResolveTransientService2(context),
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }
}
