using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.DictionaryFunctionPointers;

internal static class DictionaryFunctionPointersGeneratedServiceExports
{
    public static IReadOnlyList<DictionaryFunctionPointersServiceExport> Exports { get; } =
    [
        new(typeof(SingletonService1), GetFactoryPointer(nameof(ResolveSingletonService1Export))),
        new(typeof(SingletonService2), GetFactoryPointer(nameof(ResolveSingletonService2Export))),
        new(typeof(SingletonService3), GetFactoryPointer(nameof(ResolveSingletonService3Export))),

        new(typeof(ScopedService3), GetFactoryPointer(nameof(ResolveScopedService3Export))),
        new(typeof(ScopedService2), GetFactoryPointer(nameof(ResolveScopedService2Export))),
        new(typeof(ScopedService1), GetFactoryPointer(nameof(ResolveScopedService1Export))),

        new(typeof(TransientService4), GetFactoryPointer(nameof(ResolveTransientService4Export))),
        new(typeof(TransientService3), GetFactoryPointer(nameof(ResolveTransientService3Export))),
        new(typeof(TransientService2), GetFactoryPointer(nameof(ResolveTransientService2Export))),
        new(typeof(TransientService1), GetFactoryPointer(nameof(ResolveTransientService1Export))),
    ];

    private static IntPtr GetFactoryPointer(string methodName)
    {
        return typeof(DictionaryFunctionPointersGeneratedServiceExports)
            .GetMethod(
                methodName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MethodHandle
            .GetFunctionPointer();
    }

    private static object ResolveSingletonService1Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveSingletonService1(context);
    }

    private static object ResolveSingletonService2Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveSingletonService2(context);
    }

    private static object ResolveSingletonService3Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveSingletonService3(context);
    }

    private static object ResolveScopedService3Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveScopedService3(context);
    }

    private static object ResolveScopedService2Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveScopedService2(context);
    }

    private static object ResolveScopedService1Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveScopedService1(context);
    }

    private static object ResolveTransientService4Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveTransientService4(context);
    }

    private static object ResolveTransientService3Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveTransientService3(context);
    }

    private static object ResolveTransientService2Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveTransientService2(context);
    }

    private static object ResolveTransientService1Export(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return ResolveTransientService1(context);
    }

    private static SingletonService1 ResolveSingletonService1(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService1),
            context,
            static _ => new SingletonService1());
    }

    private static SingletonService2 ResolveSingletonService2(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService2),
            context,
            static currentContext => new SingletonService2(ResolveSingletonService1(currentContext)));
    }

    private static SingletonService3 ResolveSingletonService3(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService3),
            context,
            static currentContext => new SingletonService3(ResolveSingletonService2(currentContext)));
    }

    private static ScopedService3 ResolveScopedService3(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService3),
            context,
            static currentContext => new ScopedService3(ResolveSingletonService3(currentContext)));
    }

    private static ScopedService2 ResolveScopedService2(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService2),
            context,
            static currentContext => new ScopedService2(
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static ScopedService1 ResolveScopedService1(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService1),
            context,
            static currentContext => new ScopedService1(
                ResolveScopedService2(currentContext),
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static TransientService4 ResolveTransientService4(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return new TransientService4(ResolveScopedService3(context));
    }

    private static TransientService3 ResolveTransientService3(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return new TransientService3(
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService2 ResolveTransientService2(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return new TransientService2(
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService1 ResolveTransientService1(
        DictionaryFunctionPointersCompileTimeProviderContext context)
    {
        return new TransientService1(
            ResolveTransientService2(context),
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }
}
