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

    // ─── Enquetes (F25) ───────────────────────────────────────────────────────

    private const string PollsBaseUrl = "api/v1/communication/polls";

    /// <inheritdoc/>
    public async Task<List<PollDto>?> GetPollsAsync(bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var url = $"{PollsBaseUrl}?onlyActive={onlyActive.ToString().ToLowerInvariant()}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<List<PollDto>>(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PollDto?> CreatePollAsync(CreatePollDto dto, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            title = dto.Title,
            description = dto.Description,
            expiresAtUtc = dto.ExpiresAtUtc?.ToUniversalTime(),
            options = dto.Options
        };

        var response = await _httpClient.PostAsJsonAsync(PollsBaseUrl, payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PollDto>(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PollDto?> SubmitPollVoteAsync(Guid pollId, SubmitPollVoteDto dto, CancellationToken cancellationToken = default)
    {
        var payload = new { optionId = dto.OptionId };
        var response = await _httpClient.PostAsJsonAsync($"{PollsBaseUrl}/{pollId}/vote", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PollDto>(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ClosePollAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"{PollsBaseUrl}/{pollId}/close", content: null, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
