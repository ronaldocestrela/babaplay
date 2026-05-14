using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed record CreateMatchEventTypeCommand(
    string Code,
    string Name,
    int Points,
    bool IsSystemDefault)
    : ICommand<Result<MatchEventTypeResponse>>;
