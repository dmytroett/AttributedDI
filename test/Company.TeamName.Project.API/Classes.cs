using AttributedDI;

namespace Company.TeamName.Project.API;

public interface IEmailService
{
    void SendEmail(string receiver, string subject, string body);
}

[RegisterAs<IEmailService>]
public class EmailService: IEmailService
{
    public void SendEmail(string receiver, string subject, string body)
    {
        throw new NotImplementedException();
    }
}

public interface ICold
{
    Task WarmUpAsync(CancellationToken token = default);
}

public interface IComplicatedSystemFacade
{
    Task InitializeAsync(CancellationToken token = default);
    
    Task TriggerPaymentAsync(decimal amount, string currency, CancellationToken token = default);
    
    Task NotifyUserAsync(string userId, string message, CancellationToken token = default);
}

[RegisterAsImplementedInterfaces]
public sealed class ComplicatedSystemFacade : IComplicatedSystemFacade, ICold, IAsyncDisposable, IDisposable
{
    public Task InitializeAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task TriggerPaymentAsync(decimal amount, string currency, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task NotifyUserAsync(string userId, string message, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task WarmUpAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

[RegisterAsSelf]
public class SimpleService
{
    public void DoWork()
    {
        throw new NotImplementedException();
    }
}