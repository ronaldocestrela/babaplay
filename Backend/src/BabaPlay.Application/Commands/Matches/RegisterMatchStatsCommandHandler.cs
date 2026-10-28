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
    private readonly IGameDayRepository? _gameDayRepository;

    public RegisterMatchStatsCommandHandler(
        IMatchRepository matchRepository,
        IGameDayRepository? gameDayRepository = null)
    {
        _matchRepository = matchRepository;
        _gameDayRepository = gameDayRepository;
    }

    public async Task<Result<RegisterMatchStatsApplicationDto>> HandleAsync(
        RegisterMatchStatsCommand command,
        CancellationToken ct = default)
    {
        var match = await _matchRepository.GetByIdAsync(command.MatchId, ct);
        if (match == null)
        {
            var matches = await _matchRepository.GetAllActiveAsync(null, ct);
            match = matches.FirstOrDefault(m => m.GameDayId == command.MatchId);
        }

        if (match == null && _gameDayRepository != null)
        {
            var gameDay = await _gameDayRepository.GetByIdAsync(command.MatchId, ct);
            if (gameDay != null)
            {
                match = BabaPlay.Domain.Entities.Match.Create(
                    gameDay.TenantId,
                    gameDay.Id,
                    Guid.Empty,
                    Guid.Empty,
                    gameDay.Name,
                    BabaPlay.Domain.Enums.MatchStatus.Completed);
                await _matchRepository.AddAsync(match, ct);
                await _matchRepository.SaveChangesAsync(ct);
            }
        }

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
