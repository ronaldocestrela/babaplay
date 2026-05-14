using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Positions;

public sealed record DeletePositionCommand(Guid PositionId)
    : ICommand<Result>;
