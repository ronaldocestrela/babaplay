using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface IRankingApiService
{
    Task<IReadOnlyList<RankingEntryDto>> GetRankingAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopScorerEntryDto>> GetTopScorersAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceEntryDto>> GetAttendanceAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> RebuildRankingAsync(CancellationToken cancellationToken = default);
}
