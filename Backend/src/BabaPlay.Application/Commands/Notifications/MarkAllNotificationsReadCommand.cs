using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Notifications;

public sealed record MarkAllNotificationsReadCommand(
    Guid UserId) : ICommand<Result>;
