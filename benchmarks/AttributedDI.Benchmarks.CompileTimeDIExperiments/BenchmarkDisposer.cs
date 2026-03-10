namespace AttributedDI.Benchmarks.CompileTimeDIExperiments;

internal static class BenchmarkDisposer
{
    public static void DisposeProvider(IServiceProvider? provider)
    {
        if (provider is null)
        {
            return;
        }

        if (provider is IAsyncDisposable asyncDisposable)
        {
            asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
            return;
        }

        if (provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
