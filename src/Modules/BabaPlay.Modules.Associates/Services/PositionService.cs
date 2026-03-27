using BabaPlay.Modules.Associates.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Associates.Services;

public sealed class PositionService
{
    private readonly ITenantRepository<Position> _repo;
    private readonly ITenantUnitOfWork _uow;

    public PositionService(ITenantRepository<Position> repo, ITenantUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<Position>>> ListAsync(CancellationToken ct)
    {
        var list = await _repo.Query().OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToListAsync(ct);
        return Result.Success<IReadOnlyList<Position>>(list);
    }

    public async Task<Result<Position>> CreateAsync(string name, int sortOrder, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<Position>("Name is required.");
        var p = new Position { Name = name.Trim(), SortOrder = sortOrder };
        await _repo.AddAsync(p, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(p);
    }
}
