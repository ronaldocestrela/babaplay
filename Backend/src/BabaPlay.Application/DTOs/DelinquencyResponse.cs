namespace BabaPlay.Application.DTOs;

public sealed record DelinquencyResponse(
    DateTime ReferenceUtc,
    decimal TotalOpenAmount,
    IReadOnlyList<DelinquencyEntryResponse> Items);
