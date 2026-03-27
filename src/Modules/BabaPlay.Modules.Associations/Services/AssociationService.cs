using BabaPlay.Modules.Associations.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Associations.Services;

public sealed class AssociationService
{
    private readonly ITenantRepository<Association> _repo;
    private readonly ITenantUnitOfWork _uow;

    public AssociationService(ITenantRepository<Association> repo, ITenantUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<Association>>> ListAsync(CancellationToken ct)
    {
        var list = await _repo.Query().OrderBy(x => x.Name).ToListAsync(ct);
        return Result.Success<IReadOnlyList<Association>>(list);
    }

    public async Task<Result<Association>> GetAsync(string id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? Result.NotFound<Association>("Association not found.") : Result.Success(entity);
    }

    public async Task<Result<Association>> UpsertSingleAsync(string? id, string name, string? address, string? regulation, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<Association>("Name is required.");

        Association entity;
        if (string.IsNullOrEmpty(id))
        {
            entity = new Association { Name = name.Trim(), Address = address, Regulation = regulation };
            await _repo.AddAsync(entity, ct);
        }
        else
        {
            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null) return Result.NotFound<Association>("Association not found.");
            entity = existing;
            entity.Name = name.Trim();
            entity.Address = address;
            entity.Regulation = regulation;
            entity.UpdatedAt = DateTime.UtcNow;
            _repo.Update(entity);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success(entity);
    }
}
