using BabaPlay.Modules.Financial.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Financial.Services;

public sealed class CashEntryService : ICashEntryService
{
    private readonly ITenantRepository<CashEntry> _repo;
    private readonly ITenantRepository<Category> _categories;
    private readonly ITenantUnitOfWork _uow;

    public CashEntryService(ITenantRepository<CashEntry> repo, ITenantRepository<Category> categories, ITenantUnitOfWork uow)
    {
        _repo = repo;
        _categories = categories;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<CashEntry>>> ListAsync(CancellationToken ct)
    {
        var list = await _repo.Query().Include(e => e.Category).OrderByDescending(e => e.EntryDate).ToListAsync(ct);
        return Result.Success<IReadOnlyList<CashEntry>>(list);
    }

    public async Task<Result<CashEntry>> CreateAsync(decimal amount, string categoryId, string? description, DateTime? entryDate, CancellationToken ct)
    {
        var category = await _categories.GetByIdAsync(categoryId, ct);
        if (category is null)
            return Result.NotFound<CashEntry>("Category not found.");

        var e = new CashEntry
        {
            Amount = amount,
            CategoryId = categoryId,
            Description = description,
            EntryDate = entryDate ?? DateTime.UtcNow
        };

        await _repo.AddAsync(e, ct);
        await _uow.SaveChangesAsync(ct);

        await RecalculateRunningBalanceAsync(ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(e);
    }

    private async Task RecalculateRunningBalanceAsync(CancellationToken ct)
    {
        var entries = await _repo.Query()
            .Include(x => x.Category)
            .OrderBy(x => x.EntryDate)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        decimal runningBalance = 0m;
        foreach (var entry in entries)
        {
            var type = entry.Category?.Type ?? CategoryType.Income;
            var normalizedAmount = Math.Abs(entry.Amount);
            runningBalance += type == CategoryType.Expense ? -normalizedAmount : normalizedAmount;
            entry.CurrentBalance = runningBalance;
            entry.UpdatedAt = DateTime.UtcNow;
            _repo.Update(entry);
        }
    }
}
