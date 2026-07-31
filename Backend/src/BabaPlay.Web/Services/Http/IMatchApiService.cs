using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface IMatchApiService
{
    Task<IReadOnlyList<GameDayDto>> GetGameDaysAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<GameDayDto?> GetGameDayByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> CreateGameDayAsync(ScheduleGameDayDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> UpdateGameDayAsync(Guid id, ScheduleGameDayDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> DeleteGameDayAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> ChangeGameDayStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchDto>> GetMatchesAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<MatchDto?> GetMatchByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> CreateMatchAsync(CreateMatchDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> UpdateMatchAsync(Guid id, UpdateMatchDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> ChangeMatchStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> DeleteMatchAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RegisterMatchStatsDto?> GetMatchStatsAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> RegisterMatchStatsAsync(Guid matchId, RegisterMatchStatsDto dto, CancellationToken cancellationToken = default);

    Task<MvpResultDto?> GetMvpResultsAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> SubmitMvpVoteAsync(Guid matchId, SubmitMvpVoteDto dto, CancellationToken cancellationToken = default);
}


