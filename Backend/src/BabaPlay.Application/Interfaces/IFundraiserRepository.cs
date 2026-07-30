using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface IFundraiserRepository
{
    Task<Fundraiser?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Fundraiser>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default);

    Task AddAsync(Fundraiser fundraiser, CancellationToken ct = default);

    Task UpdateAsync(Fundraiser fundraiser, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
