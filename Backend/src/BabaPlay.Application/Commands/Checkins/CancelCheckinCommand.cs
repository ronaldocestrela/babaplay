using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Checkins;

public sealed record CancelCheckinCommand(Guid CheckinId) : ICommand<Result>;
