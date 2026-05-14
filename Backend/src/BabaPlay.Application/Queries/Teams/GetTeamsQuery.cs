using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Teams;

public sealed record GetTeamsQuery : IQuery<Result<IReadOnlyList<TeamResponse>>>;
