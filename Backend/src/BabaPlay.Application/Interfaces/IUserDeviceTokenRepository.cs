using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface IUserDeviceTokenRepository
{
    Task<UserDeviceToken?> GetByUserAndDeviceAsync(Guid tenantId, Guid userId, string deviceId, CancellationToken ct = default);

    Task<IReadOnlyList<UserDeviceToken>> GetByUserAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    Task AddAsync(UserDeviceToken token, CancellationToken ct = default);

    Task UpdateAsync(UserDeviceToken token, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
