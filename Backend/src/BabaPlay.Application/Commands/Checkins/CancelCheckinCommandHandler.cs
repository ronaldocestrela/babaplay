using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Checkins;

public sealed class CancelCheckinCommandHandler : ICommandHandler<CancelCheckinCommand, Result>
{
    private readonly ICheckinRepository _checkinRepository;
    private readonly ICheckinRealtimeNotifier _checkinRealtimeNotifier;

    public CancelCheckinCommandHandler(ICheckinRepository checkinRepository, ICheckinRealtimeNotifier checkinRealtimeNotifier)
    {
        _checkinRepository = checkinRepository;
        _checkinRealtimeNotifier = checkinRealtimeNotifier;
    }

    public async Task<Result> HandleAsync(CancelCheckinCommand cmd, CancellationToken ct = default)
    {
        var checkin = await _checkinRepository.GetByIdAsync(cmd.CheckinId, ct);
        if (checkin is null)
            return Result.Fail("CHECKIN_NOT_FOUND", "Check-in was not found.");

        if (!checkin.IsActive)
            return Result.Ok();

        checkin.Deactivate(DateTime.UtcNow);

        await _checkinRepository.UpdateAsync(checkin, ct);
        await _checkinRepository.SaveChangesAsync(ct);

        var activeCount = await _checkinRepository.CountActiveByGameDayAsync(checkin.GameDayId, ct);
        await _checkinRealtimeNotifier.NotifyCheckinUndoneAsync(checkin.GameDayId, checkin.PlayerId, ct);
        await _checkinRealtimeNotifier.NotifyCheckinCountUpdatedAsync(checkin.GameDayId, activeCount, ct);

        return Result.Ok();
    }
}
