using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Matches;

public sealed record DeleteMatchCommand(Guid MatchId) : ICommand<Result>;