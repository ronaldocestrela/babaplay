using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Positions;

public sealed record CreatePositionCommand(
    string Code,
    string Name,
    string? Description) : ICommand<Result<PositionResponse>>;
