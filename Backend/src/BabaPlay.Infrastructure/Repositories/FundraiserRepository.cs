using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class FundraiserRepository : IFundraiserRepository
{
    private readonly AppDbContext _context;

    public FundraiserRepository(AppDbContext context) => _context = context;

    public async Task<Fundraiser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Fundraisers.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);

    public async Task<IReadOnlyList<Fundraiser>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default)
    {
        var query = _context.Fundraisers.AsNoTracking().Where(x => x.IsActive);
        if (onlyActive.HasValue && onlyActive.Value)
        {
            query = query.Where(x => x.Status == Domain.Enums.FundraiserStatus.Active);
        }

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    }

    public async Task AddAsync(Fundraiser fundraiser, CancellationToken ct = default)
    {
        await _context.Fundraisers.AddAsync(fundraiser, ct);
        await SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Fundraiser fundraiser, CancellationToken ct = default)
    {
        _context.Fundraisers.Update(fundraiser);
        await SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
