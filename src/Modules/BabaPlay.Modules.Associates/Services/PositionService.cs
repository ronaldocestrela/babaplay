using BabaPlay.Modules.Associates.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Associates.Services;

public sealed class PositionService
{
    private readonly ITenantRepository<Position> _repo;
    private readonly ITenantRepository<AssociatePosition> _associatePositions;
    private readonly ITenantUnitOfWork _uow;

    public PositionService(
        ITenantRepository<Position> repo,
        ITenantRepository<AssociatePosition> associatePositions,
        ITenantUnitOfWork uow)
    {
        _repo = repo;
        _associatePositions = associatePositions;
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

    public async Task<Result<Position>> UpdateAsync(string id, string name, int sortOrder, CancellationToken ct)
    {
        var position = await _repo.GetByIdAsync(id, ct);
        if (position is null) return Result.NotFound<Position>("Position not found.");
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<Position>("Name is required.");

        position.Name = name.Trim();
        position.SortOrder = sortOrder;
        position.UpdatedAt = DateTime.UtcNow;
        _repo.Update(position);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(position);
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken ct)
    {
        var position = await _repo.GetByIdAsync(id, ct);
        if (position is null) return Result.Failure("Position not found.", ResultStatus.NotFound);

        var inUse = await _associatePositions.Query().AnyAsync(ap => ap.PositionId == id, ct);
        if (inUse)
            return Result.Failure("Position is assigned to one or more associates.", ResultStatus.Conflict);

        _repo.Remove(position);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
