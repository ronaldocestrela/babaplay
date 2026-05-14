namespace BabaPlay.Application.Interfaces;

public interface IEmailDispatchQueue
{
    Task EnqueueAsync(EmailMessage message, CancellationToken ct = default);

    Task<EmailMessage> DequeueAsync(CancellationToken ct = default);
}
