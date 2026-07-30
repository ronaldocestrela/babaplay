using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class RankingApiService : IRankingApiService
{
    private readonly HttpClient _httpClient;

    public RankingApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<RankingEntryDto>> GetRankingAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/ranking?page={page}&pageSize={pageSize}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<RankingEntryDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<RankingEntryDto>>(cancellationToken: cancellationToken)
            ?? new List<RankingEntryDto>();
    }

    public async Task<IReadOnlyList<TopScorerEntryDto>> GetTopScorersAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/ranking/top-scorers?page={page}&pageSize={pageSize}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<TopScorerEntryDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<TopScorerEntryDto>>(cancellationToken: cancellationToken)
            ?? new List<TopScorerEntryDto>();
    }

    public async Task<IReadOnlyList<AttendanceEntryDto>> GetAttendanceAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/ranking/attendance?page={page}&pageSize={pageSize}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<AttendanceEntryDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AttendanceEntryDto>>(cancellationToken: cancellationToken)
            ?? new List<AttendanceEntryDto>();
    }

    public async Task<(bool Success, string? ErrorMessage)> RebuildRankingAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/ranking/rebuild", new { }, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao recalcular ranking.");
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
