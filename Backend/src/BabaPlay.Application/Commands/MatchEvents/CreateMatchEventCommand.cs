using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed record CreateMatchEventCommand(
    Guid MatchId,
    Guid TeamId,
    Guid PlayerId,
    Guid MatchEventTypeId,
    int Minute,
    string? Notes)
    : ICommand<Result<MatchEventResponse>>;
