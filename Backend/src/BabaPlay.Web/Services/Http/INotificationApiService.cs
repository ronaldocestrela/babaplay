using System;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

/// <summary>
/// HTTP service interface for the Notification Hub (F26 — App Notifications).
/// </summary>
public interface INotificationApiService
{
    /// <summary>Returns notification summary including unread count and recent notifications list.</summary>
    Task<NotificationSummaryDto?> GetNotificationsAsync(bool onlyUnread = false, int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>Marks a specific notification as read.</summary>
    Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>Marks all notifications for the current user as read.</summary>
    Task<bool> MarkAllAsReadAsync(CancellationToken cancellationToken = default);
}
