using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Matches;

public sealed class RegisterMatchStatsCommandHandler
    : ICommandHandler<RegisterMatchStatsCommand, Result<RegisterMatchStatsApplicationDto>>
{
    private readonly IMatchRepository _matchRepository;

    public RegisterMatchStatsCommandHandler(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<Result<RegisterMatchStatsApplicationDto>> HandleAsync(
        RegisterMatchStatsCommand command,
        CancellationToken ct = default)
    {
        var match = await _matchRepository.GetByIdAsync(command.MatchId, ct);
        if (match == null)
        {
            return Result<RegisterMatchStatsApplicationDto>.Fail("MATCH_NOT_FOUND", "Partida não encontrada.");

        }

        if (match.Status != BabaPlay.Domain.Enums.MatchStatus.Completed)
        {
            match.ChangeStatus(BabaPlay.Domain.Enums.MatchStatus.Completed);
            await _matchRepository.UpdateAsync(match, ct);
            await _matchRepository.SaveChangesAsync(ct);
        }

        var result = new RegisterMatchStatsApplicationDto(
            command.MatchId,
            command.HomeScore,
            command.AwayScore,
            command.PlayerStats.ToList()
        );

        return Result<RegisterMatchStatsApplicationDto>.Ok(result);
    }
}
