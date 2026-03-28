using BabaPlay.Modules.Financial.Entities;
using BabaPlay.SharedKernel.Results;

namespace BabaPlay.Modules.Financial.Services;

public interface ICashEntryService
{
    Task<Result<IReadOnlyList<CashEntry>>> ListAsync(CancellationToken ct);
    Task<Result<CashEntry>> CreateAsync(decimal amount, string categoryId, string? description, DateTime? entryDate, CancellationToken ct);
}
