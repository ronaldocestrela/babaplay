using BabaPlay.Modules.Associates.Entities;
using BabaPlay.Modules.CheckIns.Entities;
using BabaPlay.Modules.MatchReports.Dtos;
using BabaPlay.Modules.MatchReports.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.MatchReports.Services;

public sealed class MatchReportService
{
    private readonly ITenantRepository<CheckInSession> _sessions;
    private readonly ITenantRepository<Associate> _associates;
    private readonly ITenantRepository<MatchReport> _reports;
    private readonly ITenantRepository<MatchReportGame> _games;
    private readonly ITenantRepository<MatchReportPlayerStat> _playerStats;
    private readonly ITenantUnitOfWork _uow;

    public MatchReportService(
        ITenantRepository<CheckInSession> sessions,
        ITenantRepository<Associate> associates,
        ITenantRepository<MatchReport> reports,
        ITenantRepository<MatchReportGame> games,
        ITenantRepository<MatchReportPlayerStat> playerStats,
        ITenantUnitOfWork uow)
    {
        _sessions = sessions;
        _associates = associates;
        _reports = reports;
        _games = games;
        _playerStats = playerStats;
        _uow = uow;
    }

    public async Task<Result<MatchReportResponse>> GetBySessionAsync(string sessionId, CancellationToken ct)
    {
        var report = await _reports.Query().FirstOrDefaultAsync(x => x.SessionId == sessionId, ct);
        if (report is null)
            return Result.NotFound<MatchReportResponse>("Match report not found for session.");

        return Result.Success(await BuildResponseAsync(report, ct));
    }

    public async Task<Result<MatchReportResponse>> UpsertAsync(
        string sessionId,
        string? notes,
        IReadOnlyList<MatchReportGameInput> games,
        string? userId,
        bool isAdmin,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return Result.Invalid<MatchReportResponse>("SessionId is required.");

        if (await _sessions.GetByIdAsync(sessionId, ct) is null)
            return Result.NotFound<MatchReportResponse>("Session not found.");

        var validationErrors = ValidateGames(games);
        if (validationErrors.Count > 0)
            return Result.Invalid<MatchReportResponse>(validationErrors);

        var report = await _reports.Query().FirstOrDefaultAsync(x => x.SessionId == sessionId, ct);
        if (report is not null && report.Status == MatchReportStatus.Finalized && !isAdmin)
            return Result.Forbidden<MatchReportResponse>("Only admins can edit a finalized match report.");

        var associateIds = games.SelectMany(x => x.PlayerStats).Select(x => x.AssociateId.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToList();
        var existingAssociateIds = await _associates.Query().Where(x => associateIds.Contains(x.Id))
            .Select(x => x.Id).ToListAsync(ct);
        var missingAssociateIds = associateIds.Except(existingAssociateIds, StringComparer.Ordinal).ToList();
        if (missingAssociateIds.Count > 0)
            return Result.Invalid<MatchReportResponse>($"Associates not found in tenant: {string.Join(", ", missingAssociateIds)}");

        var now = DateTime.UtcNow;
        if (report is null)
        {
            report = new MatchReport
            {
                SessionId = sessionId,
                Notes = NormalizeOptional(notes),
                UpdatedAt = now,
            };
            await _reports.AddAsync(report, ct);
        }
        else
        {
            report.Notes = NormalizeOptional(notes);
            report.UpdatedAt = now;
            _reports.Update(report);
            await RemoveExistingChildrenAsync(report.Id, ct);
        }

        for (var gameIndex = 0; gameIndex < games.Count; gameIndex++)
        {
            var inputGame = games[gameIndex];
            var game = new MatchReportGame
            {
                MatchReportId = report.Id,
                GameNumber = gameIndex + 1,
                Title = inputGame.Title.Trim(),
                Notes = NormalizeOptional(inputGame.Notes),
            };
            await _games.AddAsync(game, ct);

            foreach (var inputStat in inputGame.PlayerStats)
            {
                await _playerStats.AddAsync(new MatchReportPlayerStat
                {
                    MatchReportGameId = game.Id,
                    AssociateId = inputStat.AssociateId.Trim(),
                    Goals = inputStat.Goals,
                    Assists = inputStat.Assists,
                    YellowCards = inputStat.YellowCards,
                    RedCards = inputStat.RedCards,
                    Observations = NormalizeOptional(inputStat.Observations),
                }, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success(await BuildResponseAsync(report, ct));
    }

    public async Task<Result<MatchReportResponse>> FinalizeAsync(string sessionId, string? userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Unauthorized<MatchReportResponse>("Authenticated user is required.");

        var report = await _reports.Query().FirstOrDefaultAsync(x => x.SessionId == sessionId, ct);
        if (report is null)
            return Result.NotFound<MatchReportResponse>("Match report not found for session.");

        if (report.Status != MatchReportStatus.Finalized)
        {
            report.Status = MatchReportStatus.Finalized;
            report.FinalizedAt = DateTime.UtcNow;
            report.FinalizedByUserId = userId;
            report.UpdatedAt = report.FinalizedAt;
            _reports.Update(report);
            await _uow.SaveChangesAsync(ct);
        }

        return Result.Success(await BuildResponseAsync(report, ct));
    }

    private static List<string> ValidateGames(IReadOnlyList<MatchReportGameInput> games)
    {
        var errors = new List<string>();
        if (games.Count == 0)
        {
            errors.Add("At least one game must be informed.");
            return errors;
        }

        for (var gameIndex = 0; gameIndex < games.Count; gameIndex++)
        {
            var game = games[gameIndex];
            if (string.IsNullOrWhiteSpace(game.Title))
                errors.Add($"Game {gameIndex + 1}: title is required.");

            var seenAssociates = new HashSet<string>(StringComparer.Ordinal);
            for (var statIndex = 0; statIndex < game.PlayerStats.Count; statIndex++)
            {
                var stat = game.PlayerStats[statIndex];
                var associateId = stat.AssociateId.Trim();
                if (string.IsNullOrWhiteSpace(associateId))
                    errors.Add($"Game {gameIndex + 1}, player {statIndex + 1}: associateId is required.");

                if (!string.IsNullOrWhiteSpace(associateId) && !seenAssociates.Add(associateId))
                    errors.Add($"Game {gameIndex + 1}: associate {associateId} is duplicated.");

                if (stat.Goals < 0 || stat.Assists < 0 || stat.YellowCards < 0 || stat.RedCards < 0)
                    errors.Add($"Game {gameIndex + 1}, associate {associateId}: stats must be non-negative.");
            }
        }

        return errors;
    }

    private async Task RemoveExistingChildrenAsync(string reportId, CancellationToken ct)
    {
        var existingGames = await _games.Query().Where(x => x.MatchReportId == reportId).ToListAsync(ct);
        if (existingGames.Count == 0)
            return;

        var gameIds = existingGames.Select(x => x.Id).ToList();
        var existingPlayerStats = await _playerStats.Query().Where(x => gameIds.Contains(x.MatchReportGameId)).ToListAsync(ct);

        foreach (var stat in existingPlayerStats)
            _playerStats.Remove(stat);

        foreach (var game in existingGames)
            _games.Remove(game);
    }

    private async Task<MatchReportResponse> BuildResponseAsync(MatchReport report, CancellationToken ct)
    {
        var games = await _games.Query().Where(x => x.MatchReportId == report.Id).OrderBy(x => x.GameNumber).ToListAsync(ct);
        var gameIds = games.Select(x => x.Id).ToList();
        var playerStats = gameIds.Count == 0
            ? new List<MatchReportPlayerStat>()
            : await _playerStats.Query().Where(x => gameIds.Contains(x.MatchReportGameId))
                .OrderBy(x => x.AssociateId).ToListAsync(ct);

        var playerStatsByGame = playerStats.GroupBy(x => x.MatchReportGameId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<MatchReportPlayerStatResponse>)g.Select(MapPlayerStat).ToList(), StringComparer.Ordinal);

        return new MatchReportResponse(
            report.Id,
            report.SessionId,
            report.Notes,
            report.Status,
            report.FinalizedAt,
            report.FinalizedByUserId,
            games.Select(game => new MatchReportGameResponse(
                game.Id,
                game.GameNumber,
                game.Title,
                game.Notes,
                playerStatsByGame.GetValueOrDefault(game.Id, Array.Empty<MatchReportPlayerStatResponse>()),
                game.CreatedAt,
                game.UpdatedAt)).ToList(),
            report.CreatedAt,
            report.UpdatedAt);
    }

    private static MatchReportPlayerStatResponse MapPlayerStat(MatchReportPlayerStat stat) =>
        new(
            stat.Id,
            stat.AssociateId,
            stat.Goals,
            stat.Assists,
            stat.YellowCards,
            stat.RedCards,
            stat.Observations,
            stat.CreatedAt,
            stat.UpdatedAt);

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}