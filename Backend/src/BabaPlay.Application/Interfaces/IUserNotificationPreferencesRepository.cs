using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface IUserNotificationPreferencesRepository
{
    Task<UserNotificationPreferences?> GetByUserAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    Task AddAsync(UserNotificationPreferences preferences, CancellationToken ct = default);

    Task UpdateAsync(UserNotificationPreferences preferences, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
