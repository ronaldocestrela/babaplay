using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface ITenantGameDayOptionRepository
{
    Task<IReadOnlyList<TenantGameDayOption>> GetByTenantAsync(Guid tenantId, bool? isActive, CancellationToken ct = default);

    Task<TenantGameDayOption?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsActiveBySlotAsync(
        Guid tenantId,
        DayOfWeek dayOfWeek,
        TimeOnly localStartTime,
        Guid? excludingId = null,
        CancellationToken ct = default);

    Task AddAsync(TenantGameDayOption option, CancellationToken ct = default);

    Task UpdateAsync(TenantGameDayOption option, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
