using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class CheckinApiService : ICheckinApiService
{
    private readonly HttpClient _httpClient;

    public CheckinApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GameDayRsvpSummaryDto?> GetGameDayRsvpSummaryAsync(Guid gameDayId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/gameday/{gameDayId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var gameDay = await response.Content.ReadFromJsonAsync<GameDayDto>(cancellationToken: cancellationToken);
        if (gameDay == null) return null;

        var checkins = await GetCheckinsByGameDayAsync(gameDayId, cancellationToken);
        var confirmedCount = checkins.Count;
        var maxPlayers = gameDay.MaxPlayers > 0 ? gameDay.MaxPlayers : 20;
        var waitlistCount = Math.Max(0, confirmedCount - maxPlayers);

        return new GameDayRsvpSummaryDto(
            gameDay.Id,
            gameDay.Name,
            gameDay.ScheduledAt,
            gameDay.Location,
            maxPlayers,
            Math.Min(confirmedCount, maxPlayers),
            waitlistCount,
            checkins.Count > 0 ? "Confirmed" : null,
            waitlistCount > 0 ? (int?)waitlistCount : null
        );
    }

    public async Task<IReadOnlyList<PlayerRsvpDetailDto>> GetRsvpListByGameDayAsync(Guid gameDayId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/checkin/gameday/{gameDayId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<PlayerRsvpDetailDto>();
        }

        var checkins = await response.Content.ReadFromJsonAsync<IReadOnlyList<CheckinDto>>(cancellationToken: cancellationToken);
        if (checkins == null) return new List<PlayerRsvpDetailDto>();

        var result = new List<PlayerRsvpDetailDto>();
        int order = 1;
        foreach (var c in checkins)
        {
            var isWaitlist = order > 20; // limite padrão se não especificado
            result.Add(new PlayerRsvpDetailDto(
                c.PlayerId,
                c.PlayerName ?? "Atleta",
                null,
                c.PhotoUrl,
                c.PositionName ?? "Atleta",
                null,
                isWaitlist ? "Waitlist" : "Confirmed",
                c.CheckedInAtUtc,
                isWaitlist ? (order - 20) : null
            ));
            order++;
        }

        return result;
    }

    public async Task<(bool Success, string? ErrorMessage, GameDayRsvpSummaryDto? UpdatedSummary)> SubmitRsvpAsync(RsvpSubmissionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.IsAttending)
        {
            var createDto = new CreateCheckinDto(
                dto.PlayerId,
                dto.GameDayId,
                DateTime.UtcNow,
                -12.9714,
                -38.5014
            );

            var (success, error) = await CreateCheckinAsync(createDto, cancellationToken);
            if (!success) return (false, error, null);
        }
        else
        {
            // Se recusar, busca checkins ativos para cancelar
            var checkins = await GetCheckinsByGameDayAsync(dto.GameDayId, cancellationToken);
            foreach (var c in checkins)
            {
                if (c.PlayerId == dto.PlayerId)
                {
                    await CancelCheckinAsync(c.Id, cancellationToken);
                }
            }
        }

        var updatedSummary = await GetGameDayRsvpSummaryAsync(dto.GameDayId, cancellationToken);
        return (true, null, updatedSummary);
    }

    public async Task<IReadOnlyList<CheckinDto>> GetCheckinsByGameDayAsync(Guid gameDayId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/checkin/gameday/{gameDayId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<CheckinDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<CheckinDto>>(cancellationToken: cancellationToken)
            ?? new List<CheckinDto>();
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateCheckinAsync(CreateCheckinDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/checkin", dto, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao realizar check-in.");
    }

    public async Task<(bool Success, string? ErrorMessage)> CancelCheckinAsync(Guid checkinId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/checkin/{checkinId}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
        return (false, errorMessage ?? "Falha ao cancelar check-in.");
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
