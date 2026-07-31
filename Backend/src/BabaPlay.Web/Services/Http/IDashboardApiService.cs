using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface IDashboardApiService
{
    Task<DashboardSummaryLoadResult> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}
