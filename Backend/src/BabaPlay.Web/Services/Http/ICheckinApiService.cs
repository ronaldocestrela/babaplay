using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface ICheckinApiService
{
    Task<GameDayRsvpSummaryDto?> GetGameDayRsvpSummaryAsync(Guid gameDayId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlayerRsvpDetailDto>> GetRsvpListByGameDayAsync(Guid gameDayId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage, GameDayRsvpSummaryDto? UpdatedSummary)> SubmitRsvpAsync(RsvpSubmissionDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CheckinDto>> GetCheckinsByGameDayAsync(Guid gameDayId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> CreateCheckinAsync(CreateCheckinDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> CancelCheckinAsync(Guid checkinId, CancellationToken cancellationToken = default);
}
