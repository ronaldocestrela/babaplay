using BabaPlay.SharedKernel.Results;

namespace BabaPlay.SharedKernel.Security;

public interface IAssociateInvitationService
{
    Task<Result<AssociateInvitationIssueResult>> CreateAsync(
        string? email,
        bool isSingleUse,
        string invitedByUserId,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task<Result<AssociateInvitationValidationResult>> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<Result> MarkAcceptedAsync(
        string token,
        string acceptedByUserId,
        CancellationToken cancellationToken = default);
}

public sealed record AssociateInvitationIssueResult(string Token, string? Email, DateTime ExpiresAt);

public sealed record AssociateInvitationValidationResult(
    string Token,
    string? Email,
    bool IsSingleUse,
    DateTime ExpiresAt);
