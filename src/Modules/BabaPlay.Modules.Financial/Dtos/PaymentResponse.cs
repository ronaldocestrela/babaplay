namespace BabaPlay.Modules.Financial.Dtos;

/// <summary>Payment payload returned by the API (avoids EF navigation cycles in JSON).</summary>
public sealed record PaymentResponse(
    string Id,
    string MembershipId,
    DateTime PaidAt,
    decimal Amount,
    string Method,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
