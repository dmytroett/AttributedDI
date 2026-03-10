using AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;

namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.TypedDelegates;

internal static class TypedDelegatesGeneratedServiceExports
{
    static TypedDelegatesGeneratedServiceExports()
    {
        InitializeTypedResolvers();
    }

    public static IReadOnlyList<TypedDelegatesServiceExport> Exports { get; } =
    [
        new(typeof(SingletonService1), static context => ResolveSingletonService1(context)),
        new(typeof(SingletonService2), static context => ResolveSingletonService2(context)),
        new(typeof(SingletonService3), static context => ResolveSingletonService3(context)),

        new(typeof(ScopedService3), static context => ResolveScopedService3(context)),
        new(typeof(ScopedService2), static context => ResolveScopedService2(context)),
        new(typeof(ScopedService1), static context => ResolveScopedService1(context)),

        new(typeof(TransientService4), static context => ResolveTransientService4(context)),
        new(typeof(TransientService3), static context => ResolveTransientService3(context)),
        new(typeof(TransientService2), static context => ResolveTransientService2(context)),
        new(typeof(TransientService1), static context => ResolveTransientService1(context)),
    ];

    private static void InitializeTypedResolvers()
    {
        TypedDelegatesCompiledResolver<SingletonService1>.Resolve = static (_, context) =>
            ResolveSingletonService1(context);

        TypedDelegatesCompiledResolver<SingletonService2>.Resolve = static (_, context) =>
            ResolveSingletonService2(context);

        TypedDelegatesCompiledResolver<SingletonService3>.Resolve = static (_, context) =>
            ResolveSingletonService3(context);

        TypedDelegatesCompiledResolver<ScopedService3>.Resolve = static (_, context) =>
            ResolveScopedService3(context);

        TypedDelegatesCompiledResolver<ScopedService2>.Resolve = static (_, context) =>
            ResolveScopedService2(context);

        TypedDelegatesCompiledResolver<ScopedService1>.Resolve = static (_, context) =>
            ResolveScopedService1(context);

        TypedDelegatesCompiledResolver<TransientService4>.Resolve = static (_, context) =>
            ResolveTransientService4(context);

        TypedDelegatesCompiledResolver<TransientService3>.Resolve = static (_, context) =>
            ResolveTransientService3(context);

        TypedDelegatesCompiledResolver<TransientService2>.Resolve = static (_, context) =>
            ResolveTransientService2(context);

        TypedDelegatesCompiledResolver<TransientService1>.Resolve = static (_, context) =>
            ResolveTransientService1(context);
    }

    private static SingletonService1 ResolveSingletonService1(
        TypedDelegatesCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService1),
            context,
            static _ => new SingletonService1());
    }

    private static SingletonService2 ResolveSingletonService2(
        TypedDelegatesCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService2),
            context,
            static currentContext => new SingletonService2(ResolveSingletonService1(currentContext)));
    }

    private static SingletonService3 ResolveSingletonService3(
        TypedDelegatesCompileTimeProviderContext context)
    {
        return context.SingletonCache.GetOrCreate(
            typeof(SingletonService3),
            context,
            static currentContext => new SingletonService3(ResolveSingletonService2(currentContext)));
    }

    private static ScopedService3 ResolveScopedService3(
        TypedDelegatesCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService3),
            context,
            static currentContext => new ScopedService3(ResolveSingletonService3(currentContext)));
    }

    private static ScopedService2 ResolveScopedService2(
        TypedDelegatesCompileTimeProviderContext context)
    {
        return context.ScopedCache.GetOrCreate(
            typeof(ScopedService2),
            context,
            static currentContext => new ScopedService2(
                ResolveScopedService3(currentContext),
                ResolveSingletonService3(currentContext)));
    }

    private static ScopedService1 ResolveScopedService1(
        TypedDelegatesCompileTimeProviderContext context)
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
        TypedDelegatesCompileTimeProviderContext context)
    {
        return new TransientService4(ResolveScopedService3(context));
    }

    private static TransientService3 ResolveTransientService3(
        TypedDelegatesCompileTimeProviderContext context)
    {
        return new TransientService3(
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService2 ResolveTransientService2(
        TypedDelegatesCompileTimeProviderContext context)
    {
        return new TransientService2(
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }

    private static TransientService1 ResolveTransientService1(
        TypedDelegatesCompileTimeProviderContext context)
    {
        return new TransientService1(
            ResolveTransientService2(context),
            ResolveTransientService3(context),
            ResolveTransientService4(context),
            ResolveScopedService3(context));
    }
}
