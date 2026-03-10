namespace AttributedDI.Benchmarks.CompileTimeDIExperiments.Services;

public sealed class SingletonService1;

public sealed class SingletonService2(SingletonService1 singletonService1)
{
    public SingletonService1 SingletonService1 { get; } = singletonService1;
}

public sealed class SingletonService3(SingletonService2 singletonService2)
{
    public SingletonService2 SingletonService2 { get; } = singletonService2;
}

public sealed class ScopedService3(SingletonService3 singletonService3)
{
    public SingletonService3 SingletonService3 { get; } = singletonService3;
}

public sealed class ScopedService2(ScopedService3 scopedService3, SingletonService3 singletonService3)
{
    public ScopedService3 ScopedService3 { get; } = scopedService3;

    public SingletonService3 SingletonService3 { get; } = singletonService3;
}

public sealed class ScopedService1(
    ScopedService2 scopedService2,
    ScopedService3 scopedService3,
    SingletonService3 singletonService3)
{
    public ScopedService2 ScopedService2 { get; } = scopedService2;

    public ScopedService3 ScopedService3 { get; } = scopedService3;

    public SingletonService3 SingletonService3 { get; } = singletonService3;
}

public sealed class TransientService4(ScopedService3 scopedService3)
{
    public ScopedService3 ScopedService3 { get; } = scopedService3;
}

public sealed class TransientService3(TransientService4 transientService4, ScopedService3 scopedService3)
{
    public TransientService4 TransientService4 { get; } = transientService4;

    public ScopedService3 ScopedService3 { get; } = scopedService3;
}

public sealed class TransientService2(
    TransientService3 transientService3,
    TransientService4 transientService4,
    ScopedService3 scopedService3)
{
    public TransientService3 TransientService3 { get; } = transientService3;

    public TransientService4 TransientService4 { get; } = transientService4;

    public ScopedService3 ScopedService3 { get; } = scopedService3;
}

public sealed class TransientService1(
    TransientService2 transientService2,
    TransientService3 transientService3,
    TransientService4 transientService4,
    ScopedService3 scopedService3)
{
    public TransientService2 TransientService2 { get; } = transientService2;

    public TransientService3 TransientService3 { get; } = transientService3;

    public TransientService4 TransientService4 { get; } = transientService4;

    public ScopedService3 ScopedService3 { get; } = scopedService3;
}
