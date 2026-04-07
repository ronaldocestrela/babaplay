using BabaPlay.SharedKernel.Results;

namespace BabaPlay.SharedKernel.Services;

public interface IEmailService
{
    Task<Result<string>> SendAsync(
        string to,
        string subject,
        string htmlTemplate,
        IReadOnlyDictionary<string, string>? placeholders = null,
        string? fromEmail = null,
        string? fromName = null,
        CancellationToken cancellationToken = default);
}