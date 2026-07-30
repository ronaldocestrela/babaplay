using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;


namespace BabaPlay.Web.Services.Http;

public sealed class PositionApiService : IPositionApiService
{
    private readonly HttpClient _httpClient;

    public PositionApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/v1/position", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<PositionDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PositionDto>>(cancellationToken: cancellationToken)
            ?? new List<PositionDto>();
    }

    public async Task<PositionDto?> GetPositionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/position/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PositionDto>(cancellationToken: cancellationToken);
    }

    public async Task<(bool Success, string? ErrorMessage)> CreatePositionAsync(CreatePositionDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/position", dto, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao criar posição.");
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdatePositionAsync(Guid id, UpdatePositionDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/v1/position/{id}", dto, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao atualizar posição.");
    }

    public async Task<(bool Success, string? ErrorMessage)> DeletePositionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/position/{id}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return (false, "Esta posição está em uso por um ou mais atletas e não pode ser excluída.");
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao excluir posição.");
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

