using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class CommunicationApiService : ICommunicationApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/communication/announcements";

    public CommunicationApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<List<AnnouncementDto>?> GetAnnouncementsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(BaseUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<List<AnnouncementDto>>(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AnnouncementDto?> CreateAnnouncementAsync(
        CreateAnnouncementDto dto,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            title = dto.Title,
            content = dto.Content,
            expiresAtUtc = dto.ExpiresAtUtc?.ToUniversalTime(),
            tags = dto.Tags
        };

        var response = await _httpClient.PostAsJsonAsync(BaseUrl, payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AnnouncementDto>(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAnnouncementReadAsync(Guid announcementId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"{BaseUrl}/{announcementId}/mark-read",
            content: null,
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
