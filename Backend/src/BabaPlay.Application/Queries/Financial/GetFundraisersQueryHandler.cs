using System.Collections.Generic;
using System.Linq;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Financial;

public sealed class GetFundraisersQueryHandler : IQueryHandler<GetFundraisersQuery, Result<IReadOnlyList<FundraiserResponse>>>
{
    private readonly IFundraiserRepository _fundraiserRepository;

    public GetFundraisersQueryHandler(IFundraiserRepository fundraiserRepository)
    {
        _fundraiserRepository = fundraiserRepository;
    }

    public async Task<Result<IReadOnlyList<FundraiserResponse>>> HandleAsync(GetFundraisersQuery query, CancellationToken ct = default)
    {
        var fundraisers = await _fundraiserRepository.GetAllAsync(query.OnlyActive, ct);

        var responses = fundraisers.Select(f =>
        {
            var progress = f.TargetAmount > 0
                ? Math.Min(100.0, Math.Round((double)(f.CollectedAmount / f.TargetAmount) * 100, 1))
                : 0;

            return new FundraiserResponse(
                f.Id,
                f.TenantId,
                f.Title,
                f.Description,
                f.TargetAmount,
                f.CollectedAmount,
                progress,
                f.DeadlineUtc,
                f.Status,
                f.ContributionsCount,
                f.CreatedAt);
        }).ToList();

        return Result<IReadOnlyList<FundraiserResponse>>.Ok(responses);
    }
}
