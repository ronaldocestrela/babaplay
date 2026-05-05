using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed record GetCashFlowQuery(DateTime FromUtc, DateTime ToUtc) : IQuery<Result<CashFlowResponse>>;
