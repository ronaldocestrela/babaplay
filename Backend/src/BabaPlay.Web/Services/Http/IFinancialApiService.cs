using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface IFinancialApiService
{
    Task<FinancialOverviewDto?> GetFinancialOverviewAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default);
}
