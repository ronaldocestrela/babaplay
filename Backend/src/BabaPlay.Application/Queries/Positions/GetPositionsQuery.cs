using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Positions;

public sealed record GetPositionsQuery : IQuery<Result<IReadOnlyList<PositionResponse>>>;
