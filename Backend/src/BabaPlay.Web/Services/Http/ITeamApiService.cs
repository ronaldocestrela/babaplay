using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface ITeamApiService
{
    Task<DrawResultDto?> GenerateBalancedTeamsAsync(GenerateBalancedTeamsDto dto, CancellationToken cancellationToken = default);
    Task<DrawResultDto?> GetTeamsByGameDayAsync(Guid gameDayId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> SaveTeamCompositionAsync(DrawResultDto dto, CancellationToken cancellationToken = default);
}
