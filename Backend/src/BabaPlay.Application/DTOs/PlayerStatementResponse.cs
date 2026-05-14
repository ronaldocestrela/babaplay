namespace BabaPlay.Application.DTOs;

public sealed record PlayerStatementResponse(
    Guid PlayerId,
    DateTime FromUtc,
    DateTime ToUtc,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalOpen,
    IReadOnlyList<PlayerStatementEntryResponse> Items);
