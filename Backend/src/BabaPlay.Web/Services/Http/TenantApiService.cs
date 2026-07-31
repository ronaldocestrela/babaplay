using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class TenantApiService : ITenantApiService
{
    private readonly HttpClient _httpClient;

    public TenantApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<TenantSummaryDto>> GetMyMembershipsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/v1/auth/me", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<TenantSummaryDto>();
        }

        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>(cancellationToken: cancellationToken);
        if (profile?.Tenants is null || profile.Tenants.Count == 0)
        {
            return Array.Empty<TenantSummaryDto>();
        }

        return profile.Tenants
            .Select(t => new TenantSummaryDto(t.Id, t.Name, t.Slug, null, t.IsOwner ? "Owner" : "Member"))
            .ToList();
    }

    public async Task<(TenantSummaryDto? Result, string? ErrorMessage)> CreateTenantAsync(CreateTenantDto dto, IBrowserFile? logoFile, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(dto.Name), "Name");
        content.Add(new StringContent(dto.Slug), "Slug");
        content.Add(new StringContent(dto.AdminEmail), "AdminEmail");
        content.Add(new StringContent(dto.AdminPassword), "AdminPassword");
        content.Add(new StringContent(dto.Street), "Street");
        content.Add(new StringContent(dto.Number), "Number");
        content.Add(new StringContent(dto.City), "City");
        content.Add(new StringContent(dto.State), "State");
        content.Add(new StringContent(dto.ZipCode), "ZipCode");

        if (!string.IsNullOrWhiteSpace(dto.Neighborhood))
            content.Add(new StringContent(dto.Neighborhood), "Neighborhood");

        if (dto.AssociationLatitude.HasValue)
            content.Add(new StringContent(dto.AssociationLatitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "AssociationLatitude");
        if (dto.AssociationLongitude.HasValue)
            content.Add(new StringContent(dto.AssociationLongitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "AssociationLongitude");

        if (logoFile is not null)
        {
            var maxAllowedSize = 5 * 1024 * 1024; // 5 MB
            var fileStream = logoFile.OpenReadStream(maxAllowedSize, cancellationToken);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(logoFile.ContentType);
            content.Add(streamContent, "Logo", logoFile.Name);
        }

        var response = await _httpClient.PostAsync("api/v1/tenant", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
            return (null, errorMessage ?? "Não foi possível cadastrar a associação.");
        }

        var result = await response.Content.ReadFromJsonAsync<TenantSummaryDto>(cancellationToken: cancellationToken);
        return (result, null);
    }

    public async Task<InviteValidationDto?> ValidateInviteAsync(string token, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/association-invite/validate?token={Uri.EscapeDataString(token)}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new InviteValidationDto(false, string.Empty, string.Empty, string.Empty, "Convite inválido ou expirado.");
        }

        return await response.Content.ReadFromJsonAsync<InviteValidationDto>(cancellationToken: cancellationToken);
    }

    public async Task<bool> AcceptInviteAsync(AcceptInviteDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/association-invite/accept", dto, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<TenantSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/v1/tenant/settings", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TenantSettingsDto>(cancellationToken: cancellationToken);
    }

    public async Task<TenantSettingsDto?> UpdateSettingsAsync(UpdateTenantSettingsDto dto, IBrowserFile? logoFile, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(dto.Name), "Name");
        content.Add(new StringContent(dto.PlayersPerTeam.ToString()), "PlayersPerTeam");

        if (!string.IsNullOrWhiteSpace(dto.Street))
            content.Add(new StringContent(dto.Street), "Street");
        if (!string.IsNullOrWhiteSpace(dto.Number))
            content.Add(new StringContent(dto.Number), "Number");
        if (!string.IsNullOrWhiteSpace(dto.Neighborhood))
            content.Add(new StringContent(dto.Neighborhood), "Neighborhood");
        if (!string.IsNullOrWhiteSpace(dto.City))
            content.Add(new StringContent(dto.City), "City");
        if (!string.IsNullOrWhiteSpace(dto.State))
            content.Add(new StringContent(dto.State), "State");
        if (!string.IsNullOrWhiteSpace(dto.ZipCode))
            content.Add(new StringContent(dto.ZipCode), "ZipCode");
        if (dto.AssociationLatitude.HasValue)
            content.Add(new StringContent(dto.AssociationLatitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "AssociationLatitude");
        if (dto.AssociationLongitude.HasValue)
            content.Add(new StringContent(dto.AssociationLongitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "AssociationLongitude");

        if (logoFile is not null)
        {
            var maxAllowedSize = 5 * 1024 * 1024; // 5 MB
            var fileStream = logoFile.OpenReadStream(maxAllowedSize, cancellationToken);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(logoFile.ContentType);
            content.Add(streamContent, "Logo", logoFile.Name);
        }

        var response = await _httpClient.PutAsync("api/v1/tenant/settings", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TenantSettingsDto>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<TenantGameDayOptionDto>> GetGameDayOptionsAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var url = "api/v1/tenant/settings/game-day-options";
        if (isActive.HasValue)
        {
            url += $"?isActive={isActive.Value.ToString().ToLower()}";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<TenantGameDayOptionDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<TenantGameDayOptionDto>>(cancellationToken: cancellationToken)
            ?? Array.Empty<TenantGameDayOptionDto>();
    }

    public async Task<TenantGameDayOptionDto?> CreateGameDayOptionAsync(CreateTenantGameDayOptionDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/tenant/settings/game-day-options", dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TenantGameDayOptionDto>(cancellationToken: cancellationToken);
    }

    public async Task<TenantGameDayOptionDto?> ChangeGameDayOptionStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/v1/tenant/settings/game-day-options/{id}/status", new { IsActive = isActive }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TenantGameDayOptionDto>(cancellationToken: cancellationToken);
    }

    private static async Task<string?> ExtractErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(cancellationToken: cancellationToken);
            if (problem is not null)
            {
                if (!string.IsNullOrWhiteSpace(problem.Title) && problem.Title.StartsWith("TENANT_", StringComparison.Ordinal))
                    return problem.Title;
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

