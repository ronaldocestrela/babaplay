using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Queries.Financial;

public sealed class GetInvoicesQueryHandler : IQueryHandler<GetInvoicesQuery, Result<IReadOnlyList<InvoiceResponse>>>
{
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;

    public GetInvoicesQueryHandler(IPlayerMonthlyFeeRepository monthlyFeeRepository)
    {
        _monthlyFeeRepository = monthlyFeeRepository;
    }

    public async Task<Result<IReadOnlyList<InvoiceResponse>>> HandleAsync(GetInvoicesQuery query, CancellationToken ct = default)
    {
        if (query.Year.HasValue && (query.Year.Value < 1 || query.Year.Value > 9999))
            return Result<IReadOnlyList<InvoiceResponse>>.Fail("INVALID_COMPETENCE", "Year must be between 1 and 9999.");

        if (query.Month.HasValue && (query.Month.Value < 1 || query.Month.Value > 12))
            return Result<IReadOnlyList<InvoiceResponse>>.Fail("INVALID_COMPETENCE", "Month must be between 1 and 12.");

        var fees = await _monthlyFeeRepository.GetInvoicesAsync(query.Year, query.Month, query.PlayerId, query.Status, ct);
        var now = DateTime.UtcNow;

        var items = fees.Select(x =>
        {
            var status = x.Status;
            if (status == MonthlyFeeStatus.Open && x.DueDateUtc < now)
            {
                status = MonthlyFeeStatus.Overdue;
            }

            var openAmount = x.Amount - x.PaidAmount;

            return new InvoiceResponse(
                x.Id,
                x.TenantId,
                x.PlayerId,
                x.Year,
                x.Month,
                x.Amount,
                x.PaidAmount,
                openAmount,
                x.DueDateUtc,
                x.PaidAtUtc,
                status,
                x.Description,
                x.IsActive,
                x.CreatedAt);
        }).ToList();

        return Result<IReadOnlyList<InvoiceResponse>>.Ok(items);
    }
}
