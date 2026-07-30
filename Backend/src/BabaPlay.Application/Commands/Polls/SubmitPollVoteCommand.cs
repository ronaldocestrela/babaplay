using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Polls;

public sealed record SubmitPollVoteCommand(
    Guid PollId,
    Guid OptionId,
    string UserId) : ICommand<Result<PollResponse>>;
