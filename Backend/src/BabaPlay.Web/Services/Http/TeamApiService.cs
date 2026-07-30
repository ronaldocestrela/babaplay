using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class TeamApiService : ITeamApiService
{
    private readonly HttpClient _httpClient;

    public TeamApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DrawResultDto?> GenerateBalancedTeamsAsync(GenerateBalancedTeamsDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/team/draw", dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<DrawResultDto>(cancellationToken: cancellationToken);
    }

    public async Task<DrawResultDto?> GetTeamsByGameDayAsync(Guid gameDayId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/team/gameday/{gameDayId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<DrawResultDto>(cancellationToken: cancellationToken);
    }

    public async Task<(bool Success, string? ErrorMessage)> SaveTeamCompositionAsync(DrawResultDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/team/save-composition", dto, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao salvar composição dos times.");
    }

    private static async Task<string?> ExtractErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(cancellationToken: cancellationToken);
            if (problem is not null)
            {
                if (!string.IsNullOrWhiteSpace(problem.Detail)) return problem.Detail;
                if (!string.IsNullOrWhiteSpace(problem.Title)) return problem.Title;
            }
        }
        catch
        {
            // Ignore JSON parse failure
        }
        return null;
    }

    private sealed record ApiProblemDetails(string? Title, string? Detail);
}
