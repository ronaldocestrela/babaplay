using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Players;

/// <summary>Query to retrieve a single player by id within the current tenant.</summary>
public sealed record GetPlayerQuery(Guid PlayerId) : IQuery<Result<PlayerResponse>>;
