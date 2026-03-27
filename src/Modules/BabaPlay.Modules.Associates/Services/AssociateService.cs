using BabaPlay.Modules.Associates.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Associates.Services;

public sealed class AssociateService
{
    private readonly ITenantRepository<Associate> _associates;
    private readonly ITenantRepository<AssociatePosition> _links;
    private readonly ITenantRepository<Position> _positions;
    private readonly ITenantUnitOfWork _uow;

    public AssociateService(
        ITenantRepository<Associate> associates,
        ITenantRepository<AssociatePosition> links,
        ITenantRepository<Position> positions,
        ITenantUnitOfWork uow)
    {
        _associates = associates;
        _links = links;
        _positions = positions;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<Associate>>> ListAsync(CancellationToken ct)
    {
        var list = await _associates.Query().Include(a => a.Positions).ThenInclude(ap => ap.Position)
            .OrderBy(a => a.Name).ToListAsync(ct);
        return Result.Success<IReadOnlyList<Associate>>(list);
    }

    public async Task<Result<Associate>> GetAsync(string id, CancellationToken ct)
    {
        var a = await _associates.Query().Include(x => x.Positions).ThenInclude(l => l.Position)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return a is null ? Result.NotFound<Associate>("Associate not found.") : Result.Success(a);
    }

    public async Task<Result<Associate>> CreateAsync(string name, string? email, string? phone, IReadOnlyList<string> positionIds, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<Associate>("Name is required.");
        var validation = await ValidatePositionsAsync(positionIds, ct);
        if (!validation.IsSuccess) return Result.Invalid<Associate>(validation.Errors);

        var associate = new Associate { Name = name.Trim(), Email = email, Phone = phone };
        await _associates.AddAsync(associate, ct);
        await _uow.SaveChangesAsync(ct);

        foreach (var pid in positionIds.Distinct())
        {
            await _links.AddAsync(new AssociatePosition { AssociateId = associate.Id, PositionId = pid }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return await GetAsync(associate.Id, ct);
    }

    public async Task<Result<Associate>> UpdateAsync(string id, string name, string? email, string? phone, IReadOnlyList<string> positionIds, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<Associate>("Name is required.");
        var validation = await ValidatePositionsAsync(positionIds, ct);
        if (!validation.IsSuccess) return Result.Invalid<Associate>(validation.Errors);

        var associate = await _associates.GetByIdAsync(id, ct);
        if (associate is null) return Result.NotFound<Associate>("Associate not found.");
        associate.Name = name.Trim();
        associate.Email = email;
        associate.Phone = phone;
        associate.UpdatedAt = DateTime.UtcNow;
        _associates.Update(associate);

        var existing = await _links.Query().Where(x => x.AssociateId == id).ToListAsync(ct);
        foreach (var e in existing)
            _links.Remove(e);
        await _uow.SaveChangesAsync(ct);

        foreach (var pid in positionIds.Distinct())
        {
            await _links.AddAsync(new AssociatePosition { AssociateId = associate.Id, PositionId = pid }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return await GetAsync(associate.Id, ct);
    }

    private async Task<Result> ValidatePositionsAsync(IReadOnlyList<string> positionIds, CancellationToken ct)
    {
        var ids = positionIds.Distinct().ToList();
        if (ids.Count is < 1 or > 3)
            return Result.Failure("Associate must have between 1 and 3 positions.", ResultStatus.Invalid);

        foreach (var pid in ids)
        {
            if (await _positions.GetByIdAsync(pid, ct) is null)
                return Result.Failure($"Position {pid} not found.", ResultStatus.Invalid);
        }

        return Result.Success();
    }
}
