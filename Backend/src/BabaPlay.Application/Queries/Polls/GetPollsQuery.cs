using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Polls;

public sealed record GetPollsQuery(
    bool OnlyActive = true,
    string? RequestingUserId = null) : IQuery<Result<IReadOnlyList<PollResponse>>>;
