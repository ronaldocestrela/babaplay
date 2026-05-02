using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Players;

/// <summary>Query to retrieve all active players within the current tenant.</summary>
public sealed record GetPlayersQuery() : IQuery<Result<IReadOnlyList<PlayerResponse>>>;
