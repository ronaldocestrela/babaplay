using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed record GetDefaultersListQuery(DateTime? ReferenceUtc = null) : IQuery<Result<DefaultersListResponse>>;
