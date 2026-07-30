using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface IDashboardApiService
{
    Task<DashboardSummaryDto?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}
