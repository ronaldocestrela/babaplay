namespace BabaPlay.Application.DTOs;

/// <summary>
/// Response DTO for a single option within a poll.
/// </summary>
public sealed record PollOptionResponse(
    Guid Id,
    string Text,
    int Order,
    int VotesCount,
    double Percentage);

/// <summary>
/// Response DTO for a Poll, including option breakdowns, user vote status, and total vote count.
/// </summary>
public sealed record PollResponse(
    Guid Id,
    Guid TenantId,
    Guid AuthorId,
    string AuthorName,
    string Title,
    string? Description,
    DateTime? ExpiresAtUtc,
    bool IsClosed,
    bool HasVoted,
    Guid? VotedOptionId,
    int TotalVotes,
    IReadOnlyList<PollOptionResponse> Options,
    DateTime CreatedAt);
