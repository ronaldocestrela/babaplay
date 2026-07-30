using System;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Matches;

public sealed class SubmitMvpVoteCommandHandler
    : ICommandHandler<SubmitMvpVoteCommand, Result>
{
    private readonly IMatchRepository _matchRepository;

    public SubmitMvpVoteCommandHandler(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<Result> HandleAsync(
        SubmitMvpVoteCommand command,
        CancellationToken ct = default)
    {
        if (command.VoterPlayerId == command.CandidatePlayerId)
        {
            return Result.Fail("SELF_VOTE_NOT_ALLOWED", "Você não pode votar em si mesmo para Craque do Jogo.");
        }

        var match = await _matchRepository.GetByIdAsync(command.MatchId, ct);
        if (match == null)
        {
            return Result.Fail("MATCH_NOT_FOUND", "Partida não encontrada.");
        }

        return Result.Ok();
    }
}
