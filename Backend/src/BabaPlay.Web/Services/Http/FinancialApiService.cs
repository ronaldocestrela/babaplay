using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class FinancialApiService : IFinancialApiService
{
    private readonly HttpClient _httpClient;

    public FinancialApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FinancialOverviewDto?> GetFinancialOverviewAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default)
    {
        var url = "api/v1/financial/overview";
        if (year.HasValue && month.HasValue)
        {
            url += $"?year={year.Value}&month={month.Value}";
        }
        else if (year.HasValue)
        {
            url += $"?year={year.Value}";
        }
        else if (month.HasValue)
        {
            url += $"?month={month.Value}";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<FinancialOverviewDto>(cancellationToken: cancellationToken);
    }
}
