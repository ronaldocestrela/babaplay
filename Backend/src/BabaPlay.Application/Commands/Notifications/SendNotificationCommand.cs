using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.Notifications;

public sealed record SendNotificationCommand(
    string Title,
    string Message,
    NotificationType Type = NotificationType.System,
    IReadOnlyList<Guid>? TargetUserIds = null) : ICommand<Result<SendNotificationResponse>>;
