using System;
using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Matches;

public sealed record SubmitMvpVoteCommand(
    Guid MatchId,
    Guid VoterPlayerId,
    Guid CandidatePlayerId) : ICommand<Result>;
