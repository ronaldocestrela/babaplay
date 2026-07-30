using System;
using System.Collections.Generic;
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
        return profile?.Memberships ?? Array.Empty<TenantSummaryDto>();
    }

    public async Task<TenantSummaryDto?> CreateTenantAsync(CreateTenantDto dto, IBrowserFile? logoFile, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(dto.Name), "Name");
        content.Add(new StringContent(dto.Slug), "Slug");
        content.Add(new StringContent(dto.AdminEmail), "AdminEmail");
        content.Add(new StringContent(dto.AdminPassword), "AdminPassword");
        content.Add(new StringContent(dto.City), "City");
        content.Add(new StringContent(dto.State), "State");

        if (!string.IsNullOrWhiteSpace(dto.Street))
            content.Add(new StringContent(dto.Street), "Street");
        if (!string.IsNullOrWhiteSpace(dto.Number))
            content.Add(new StringContent(dto.Number), "Number");
        if (!string.IsNullOrWhiteSpace(dto.Neighborhood))
            content.Add(new StringContent(dto.Neighborhood), "Neighborhood");
        if (!string.IsNullOrWhiteSpace(dto.ZipCode))
            content.Add(new StringContent(dto.ZipCode), "ZipCode");

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
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TenantSummaryDto>(cancellationToken: cancellationToken);
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
}
