using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed record DeleteMatchEventTypeCommand(Guid MatchEventTypeId) : ICommand<Result>;
