using BabaPlay.Modules.MatchReports.Entities;

namespace BabaPlay.Modules.MatchReports.Dtos;

public sealed record MatchReportPlayerStatInput(
    string AssociateId,
    int Goals,
    int Assists,
    int YellowCards,
    int RedCards,
    string? Observations);

public sealed record MatchReportGameInput(
    string Title,
    string? Notes,
    IReadOnlyList<MatchReportPlayerStatInput> PlayerStats);

public sealed record MatchReportPlayerStatResponse(
    string Id,
    string AssociateId,
    int Goals,
    int Assists,
    int YellowCards,
    int RedCards,
    string? Observations,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record MatchReportGameResponse(
    string Id,
    int GameNumber,
    string Title,
    string? Notes,
    IReadOnlyList<MatchReportPlayerStatResponse> PlayerStats,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record MatchReportResponse(
    string Id,
    string SessionId,
    string? Notes,
    MatchReportStatus Status,
    DateTime? FinalizedAt,
    string? FinalizedByUserId,
    IReadOnlyList<MatchReportGameResponse> Games,
    DateTime CreatedAt,
    DateTime? UpdatedAt);