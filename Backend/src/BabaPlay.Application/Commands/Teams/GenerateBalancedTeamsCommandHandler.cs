using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Teams;

public sealed class GenerateBalancedTeamsCommandHandler
    : ICommandHandler<GenerateBalancedTeamsCommand, Result<DrawResultApplicationDto>>
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ICheckinRepository _checkinRepository;

    public GenerateBalancedTeamsCommandHandler(
        IPlayerRepository playerRepository,
        ICheckinRepository checkinRepository)
    {
        _playerRepository = playerRepository;
        _checkinRepository = checkinRepository;
    }

    public async Task<Result<DrawResultApplicationDto>> HandleAsync(
        GenerateBalancedTeamsCommand command,
        CancellationToken ct = default)
    {
        var checkins = await _checkinRepository.GetActiveByGameDayAsync(command.GameDayId, ct);

        var activePlayers = await _playerRepository.GetAllActiveAsync(ct);

        var numTeams = Math.Max(2, Math.Min(4, command.NumberOfTeams));
        var teamConfigs = new List<(string Name, string ColorHex, string BadgeClass)>
        {
            ("Time Amarelo 🟡", "#f59e0b", "bg-warning text-dark"),
            ("Time Azul 🔵", "#3b82f6", "bg-primary text-white"),
            ("Time Vermelho 🔴", "#ef4444", "bg-danger text-white"),
            ("Time Verde 🟢", "#10b981", "bg-success text-white")
        };

        var teamBuckets = new List<DrawnTeamApplicationDto>();
        for (int i = 0; i < numTeams; i++)
        {
            var teamId = Guid.NewGuid();
            var cfg = teamConfigs[i % teamConfigs.Count];
            teamBuckets.Add(new DrawnTeamApplicationDto(
                teamId,
                cfg.Name,
                cfg.ColorHex,
                cfg.BadgeClass,
                new List<DrawnPlayerApplicationDto>(),
                0.0
            ));
        }

        // Se não houver checkins suficientes no repositório, usa atletas ativos cadastrados para demonstração/sorteio
        var playerPool = activePlayers.Select((p, idx) => new
        {
            PlayerId = p.Id,
            Name = p.Name,
            Nickname = p.Nickname,
            PositionName = (idx % 4) switch { 0 => "Goleiro", 1 => "Zagueiro", 2 => "Meia", _ => "Atacante" },
            RatingStars = (idx % 5) + 1
        }).ToList();

        if (playerPool.Count == 0)
        {
            // Dummy pool if empty
            for (int i = 1; i <= 14; i++)
            {
                playerPool.Add(new
                {
                    PlayerId = Guid.NewGuid(),
                    Name = $"Atleta {i}",
                    Nickname = (string?)null,
                    PositionName = (i % 4) switch { 0 => "Goleiro", 1 => "Zagueiro", 2 => "Meia", _ => "Atacante" },
                    RatingStars = (i % 3) + 3
                });
            }
        }

        // Separar goleiros de linha
        var goalkeepers = playerPool.Where(p => p.PositionName == "Goleiro").ToList();
        var outfieldPlayers = playerPool.Where(p => p.PositionName != "Goleiro")
                                        .OrderByDescending(p => p.RatingStars)
                                        .ToList();

        // 1. Distribuir Goleiros
        for (int i = 0; i < goalkeepers.Count; i++)
        {
            var targetTeam = teamBuckets[i % numTeams];
            targetTeam.Players.Add(new DrawnPlayerApplicationDto(
                goalkeepers[i].PlayerId,
                goalkeepers[i].Name,
                goalkeepers[i].Nickname,
                goalkeepers[i].PositionName,
                goalkeepers[i].RatingStars,
                null,
                targetTeam.TeamId
            ));
        }

        // 2. Distribuir Jogadores de Linha via Snake Draft
        bool goingForward = true;
        int currentTeamIdx = 0;
        foreach (var player in outfieldPlayers)
        {
            var targetTeam = teamBuckets[currentTeamIdx];
            targetTeam.Players.Add(new DrawnPlayerApplicationDto(
                player.PlayerId,
                player.Name,
                player.Nickname,
                player.PositionName,
                player.RatingStars,
                null,
                targetTeam.TeamId
            ));

            if (goingForward)
            {
                currentTeamIdx++;
                if (currentTeamIdx >= numTeams)
                {
                    currentTeamIdx = numTeams - 1;
                    goingForward = false;
                }
            }
            else
            {
                currentTeamIdx--;
                if (currentTeamIdx < 0)
                {
                    currentTeamIdx = 0;
                    goingForward = true;
                }
            }
        }

        // Recalcular média de estrelas
        var finalTeams = teamBuckets.Select(t =>
        {
            var avg = t.Players.Count > 0 ? Math.Round(t.Players.Average(p => p.RatingStars), 1) : 0.0;
            return t with { AverageRating = avg };
        }).ToList();

        var result = new DrawResultApplicationDto(
            command.GameDayId,
            finalTeams,
            playerPool.Count
        );

        return Result<DrawResultApplicationDto>.Ok(result);
    }
}
