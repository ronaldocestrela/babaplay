using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class NotificationApiService : INotificationApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api/v1/notifications";

    public NotificationApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<NotificationSummaryDto?> GetNotificationsAsync(
        bool onlyUnread = false,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}?onlyUnread={onlyUnread.ToString().ToLowerInvariant()}&limit={limit}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<NotificationSummaryDto>(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsync($"{BaseUrl}/{notificationId}/read", content: null, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsync($"{BaseUrl}/read-all", content: null, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
