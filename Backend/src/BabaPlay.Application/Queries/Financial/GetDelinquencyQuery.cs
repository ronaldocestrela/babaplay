using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed record GetDelinquencyQuery(DateTime ReferenceUtc) : IQuery<Result<DelinquencyResponse>>;
