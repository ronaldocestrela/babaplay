using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Scores;

public sealed record RebuildTenantRankingCommand(
    DateTime? FromUtc,
    DateTime? ToUtc)
    : ICommand<Result<RebuildRankingResponse>>;