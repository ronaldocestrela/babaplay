using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Polls;

public sealed record ClosePollCommand(
    Guid PollId) : ICommand<Result>;
