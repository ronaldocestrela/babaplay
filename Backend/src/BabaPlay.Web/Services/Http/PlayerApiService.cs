using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class PlayerApiService : IPlayerApiService
{
    private readonly HttpClient _httpClient;

    public PlayerApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PlayerProfileResponseDto?> CompleteProfileAsync(CompleteProfileDto dto, IBrowserFile? photoFile, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(dto.Name), "Name");
        content.Add(new StringContent(dto.Nickname), "Nickname");
        content.Add(new StringContent(dto.PreferredFoot), "PreferredFoot");
        content.Add(new StringContent(dto.PrimaryPositionId.ToString()), "PrimaryPositionId");

        if (!string.IsNullOrWhiteSpace(dto.Phone))
            content.Add(new StringContent(dto.Phone), "Phone");

        if (dto.DateOfBirth.HasValue)
            content.Add(new StringContent(dto.DateOfBirth.Value.ToString("o")), "DateOfBirth");

        if (dto.JerseyNumber.HasValue)
            content.Add(new StringContent(dto.JerseyNumber.Value.ToString()), "JerseyNumber");

        if (photoFile is not null)
        {
            var maxAllowedSize = 5 * 1024 * 1024; // 5 MB
            var fileStream = photoFile.OpenReadStream(maxAllowedSize, cancellationToken);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(photoFile.ContentType);
            content.Add(streamContent, "Photo", photoFile.Name);
        }

        var response = await _httpClient.PutAsync("api/v1/player/profile", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PlayerProfileResponseDto>(cancellationToken: cancellationToken);
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

    public async Task<IReadOnlyList<PlayerDto>> GetPlayersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/v1/player", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<PlayerDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PlayerDto>>(cancellationToken: cancellationToken)
            ?? new List<PlayerDto>();
    }

    public async Task<PlayerDto?> GetPlayerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/player/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PlayerDto>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdatePlayerAdminAsync(Guid id, UpdatePlayerAdminDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/v1/player/{id}", dto, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePlayerRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"api/v1/role/{roleId}/users/{userId}", null, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/v1/role", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<RoleDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<RoleDto>>(cancellationToken: cancellationToken)
            ?? new List<RoleDto>();
    }

    public async Task<bool> DeletePlayerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/player/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

