using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.TenantGameDayOptions;

public sealed class GetTenantGameDayOptionsQueryHandler
    : IQueryHandler<GetTenantGameDayOptionsQuery, Result<IReadOnlyList<TenantGameDayOptionResponse>>>
{
    private readonly ITenantGameDayOptionRepository _repository;

    public GetTenantGameDayOptionsQueryHandler(ITenantGameDayOptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<TenantGameDayOptionResponse>>> HandleAsync(GetTenantGameDayOptionsQuery query, CancellationToken ct = default)
    {
        if (query.TenantId == Guid.Empty)
            return Result<IReadOnlyList<TenantGameDayOptionResponse>>.Fail("TENANT_NOT_RESOLVED", "Tenant context is required.");

        var options = await _repository.GetByTenantAsync(query.TenantId, query.IsActive, ct);

        return Result<IReadOnlyList<TenantGameDayOptionResponse>>.Ok(options
            .Select(option => new TenantGameDayOptionResponse(
                option.Id,
                option.TenantId,
                option.DayOfWeek,
                option.LocalStartTime,
                option.IsActive,
                option.CreatedAt,
                option.UpdatedAt))
            .ToList());
    }
}
