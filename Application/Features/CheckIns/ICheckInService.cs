using Domain.Entities;

namespace Application.Features.CheckIns;

public interface ICheckInService
{
    /// <summary>
    /// Registers a check-in for the current user for the current UTC day.
    /// </summary>
    /// <param name="userId">The identity user id of the associated user.</param>
    Task<DailyCheckIn> CheckInAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Returns the list of check-ins for the given date (UTC date), ordered by check-in time.
    /// </summary>
    Task<List<DailyCheckIn>> GetCheckInsByDateAsync(DateTime dateUtc, CancellationToken ct = default);

    /// <summary>
    /// Returns the teams generated from the list of check-ins for the given date.
    /// </summary>
    Task<(List<DailyCheckIn> TeamA, List<DailyCheckIn> TeamB)> GetTeamsByDateAsync(DateTime dateUtc, CancellationToken ct = default);
}
