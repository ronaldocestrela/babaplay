using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Notifications;

public sealed record MarkNotificationReadCommand(
    Guid NotificationId,
    Guid UserId) : ICommand<Result>;
