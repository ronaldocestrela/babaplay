using BabaPlay.Application.Common;

namespace BabaPlay.Application.Interfaces;

public interface IEmailService
{
    Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default);
}

public sealed record EmailMessage(
    string To,
    string Subject,
    string Html,
    string? Text = null);
