using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed record GetFinancialStatementQuery(int? Year = null, int? Month = null) : IQuery<Result<FinancialStatementResponse>>;
