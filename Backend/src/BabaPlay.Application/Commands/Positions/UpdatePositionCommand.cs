using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Positions;

public sealed record UpdatePositionCommand(
    Guid PositionId,
    string Code,
    string Name,
    string? Description) : ICommand<Result<PositionResponse>>;
