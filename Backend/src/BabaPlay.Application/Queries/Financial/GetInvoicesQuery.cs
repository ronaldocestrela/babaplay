using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Queries.Financial;

public sealed record GetInvoicesQuery(
    int? Year,
    int? Month,
    Guid? PlayerId,
    MonthlyFeeStatus? Status) : IQuery<Result<IReadOnlyList<InvoiceResponse>>>;
