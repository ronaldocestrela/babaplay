using System.Collections.Generic;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed record GetFundraisersQuery(bool? OnlyActive = null) : IQuery<Result<IReadOnlyList<FundraiserResponse>>>;
