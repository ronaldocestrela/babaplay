using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Queries.Matches;

public sealed class GetMatchStatsQueryHandler
    : IQueryHandler<GetMatchStatsQuery, Result<RegisterMatchStatsApplicationDto>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IGameDayRepository _gameDayRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetMatchStatsQueryHandler(
        IMatchRepository matchRepository,
        IGameDayRepository gameDayRepository,
        ITeamRepository teamRepository,
        IPlayerRepository playerRepository)
    {
        _matchRepository = matchRepository;
        _gameDayRepository = gameDayRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
    }

    public async Task<Result<RegisterMatchStatsApplicationDto>> HandleAsync(
        GetMatchStatsQuery query,
        CancellationToken ct = default)
    {
        var match = await _matchRepository.GetByIdAsync(query.MatchId, ct);
        if (match is null)
        {
            var matches = await _matchRepository.GetAllActiveAsync(null, ct);
            match = matches?.FirstOrDefault(m => m.GameDayId == query.MatchId);
        }

        if (match is null)
        {
            var gameDay = await _gameDayRepository.GetByIdAsync(query.MatchId, ct);
            if (gameDay is null)
            {
                return Result<RegisterMatchStatsApplicationDto>.Fail("MATCH_NOT_FOUND", "Partida não encontrada.");
            }

            return Result<RegisterMatchStatsApplicationDto>.Ok(new RegisterMatchStatsApplicationDto(
                query.MatchId,
                0,
                0,
                new List<PlayerMatchStatsApplicationDto>()));
        }

        var playerStats = new List<PlayerMatchStatsApplicationDto>();

        if (match.HomeTeamId != Guid.Empty)
        {
            var homeTeam = await _teamRepository.GetByIdAsync(match.HomeTeamId, ct);
            if (homeTeam != null)
            {
                foreach (var playerId in homeTeam.PlayerIds)
                {
                    var player = await _playerRepository.GetByIdAsync(playerId, ct);
                    if (player != null && player.IsActive)
                    {
                        playerStats.Add(new PlayerMatchStatsApplicationDto(
                            player.Id,
                            player.Name,
                            null,
                            match.HomeTeamId,
                            0,
                            0,
                            0,
                            0,
                            0));
                    }
                }
            }
        }

        if (match.AwayTeamId != Guid.Empty)
        {
            var awayTeam = await _teamRepository.GetByIdAsync(match.AwayTeamId, ct);
            if (awayTeam != null)
            {
                foreach (var playerId in awayTeam.PlayerIds)
                {
                    var player = await _playerRepository.GetByIdAsync(playerId, ct);
                    if (player != null && player.IsActive)
                    {
                        playerStats.Add(new PlayerMatchStatsApplicationDto(
                            player.Id,
                            player.Name,
                            null,
                            match.AwayTeamId,
                            0,
                            0,
                            0,
                            0,
                            0));
                    }
                }
            }
        }

        var result = new RegisterMatchStatsApplicationDto(
            match.Id,
            0,
            0,
            playerStats);

        return Result<RegisterMatchStatsApplicationDto>.Ok(result);
    }
}
