using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Notifications;

public sealed record RegisterDeviceTokenCommand(
    Guid UserId,
    string DeviceId,
    string Token,
    string Platform) : ICommand<Result<DeviceTokenResponse>>;
