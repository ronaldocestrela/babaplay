using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed record UpdateMatchEventTypeCommand(
    Guid MatchEventTypeId,
    string Code,
    string Name,
    int Points)
    : ICommand<Result<MatchEventTypeResponse>>;
