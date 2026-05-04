using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed record UpdateMatchEventCommand(
    Guid MatchEventId,
    Guid MatchEventTypeId,
    int Minute,
    string? Notes)
    : ICommand<Result<MatchEventResponse>>;
