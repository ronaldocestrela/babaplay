using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class MatchApiService : IMatchApiService
{
    private readonly HttpClient _httpClient;

    public MatchApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<GameDayDto>> GetGameDaysAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(status) ? "api/v1/gameday" : $"api/v1/gameday?status={status}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<GameDayDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<GameDayDto>>(cancellationToken: cancellationToken)
            ?? new List<GameDayDto>();
    }

    public async Task<GameDayDto?> GetGameDayByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/gameday/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<GameDayDto>(cancellationToken: cancellationToken);
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateGameDayAsync(ScheduleGameDayDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/gameday", dto, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao agendar evento de baba.");
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateGameDayAsync(Guid id, ScheduleGameDayDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/v1/gameday/{id}", dto, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao atualizar evento de baba.");
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteGameDayAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/gameday/{id}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao excluir evento de baba.");
    }

    public async Task<IReadOnlyList<MatchDto>> GetMatchesAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(status) ? "api/v1/match" : $"api/v1/match?status={status}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<MatchDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<MatchDto>>(cancellationToken: cancellationToken)
            ?? new List<MatchDto>();
    }

    public async Task<MatchDto?> GetMatchByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/match/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MatchDto>(cancellationToken: cancellationToken);
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateMatchAsync(CreateMatchDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/match", dto, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao criar partida.");
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateMatchAsync(Guid id, UpdateMatchDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/v1/match/{id}", dto, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao atualizar partida.");
    }

    public async Task<(bool Success, string? ErrorMessage)> ChangeMatchStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/v1/match/{id}/status", new ChangeMatchStatusDto(status), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao alterar status da partida.");
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteMatchAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/match/{id}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao excluir partida.");
    }

    public async Task<RegisterMatchStatsDto?> GetMatchStatsAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/match/{matchId}/stats", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RegisterMatchStatsDto>(cancellationToken: cancellationToken);
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterMatchStatsAsync(Guid matchId, RegisterMatchStatsDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/v1/match/{matchId}/stats", dto, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao registrar súmula da partida.");
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
