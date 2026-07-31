using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class DashboardApiService : IDashboardApiService
{
    private readonly HttpClient _httpClient;

    public DashboardApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DashboardSummaryLoadResult> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/v1/dashboard/summary", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>(cancellationToken: cancellationToken);
            return new DashboardSummaryLoadResult(summary, null);
        }

        var errorMessage = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                "Sessão expirada ou inválida. Faça login novamente.",
            HttpStatusCode.Forbidden =>
                "Você não tem permissão para acessar o dashboard desta associação.",
            _ =>
                "Não foi possível carregar os dados do dashboard. Verifique sua conexão ou tente novamente mais tarde.",
        };

        return new DashboardSummaryLoadResult(null, errorMessage);
    }
}
