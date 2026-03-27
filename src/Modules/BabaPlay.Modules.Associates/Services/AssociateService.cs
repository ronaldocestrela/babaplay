using BabaPlay.Modules.Associates.Dtos;
using BabaPlay.Modules.Associates.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Security;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Associates.Services;

public sealed class AssociateService
{
    private readonly ITenantRepository<Associate> _associates;
    private readonly ITenantRepository<AssociatePosition> _links;
    private readonly ITenantRepository<Position> _positions;
    private readonly ITenantUnitOfWork _uow;
    private readonly IAssociateUserProvisioner _provisioner;

    public AssociateService(
        ITenantRepository<Associate> associates,
        ITenantRepository<AssociatePosition> links,
        ITenantRepository<Position> positions,
        ITenantUnitOfWork uow,
        IAssociateUserProvisioner provisioner)
    {
        _associates = associates;
        _links = links;
        _positions = positions;
        _uow = uow;
        _provisioner = provisioner;
    }

    public async Task<Result<IReadOnlyList<AssociateResponse>>> ListAsync(CancellationToken ct)
    {
        var entities = await _associates.Query()
            .Include(a => a.Positions)
                .ThenInclude(ap => ap.Position)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);
        var list = entities.Select(MapToResponse).ToList();
        return Result.Success<IReadOnlyList<AssociateResponse>>(list);
    }

    public async Task<Result<AssociateResponse>> GetAsync(string id, CancellationToken ct)
    {
        var a = await _associates.Query()
            .Include(x => x.Positions)
                .ThenInclude(ap => ap.Position)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return a is null
            ? Result.NotFound<AssociateResponse>("Associate not found.")
            : Result.Success(MapToResponse(a));
    }

    public async Task<Result<AssociateResponse>> CreateAsync(string name, string? email, string? phone, IReadOnlyList<string> positionIds, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<AssociateResponse>("Name is required.");
        if (string.IsNullOrWhiteSpace(email)) return Result.Invalid<AssociateResponse>("Email is required.");
        var validation = await ValidatePositionsAsync(positionIds, ct);
        if (!validation.IsSuccess) return Result.Invalid<AssociateResponse>(validation.Errors);

        var emailTrimmed = email.Trim();
        var associate = new Associate { Name = name.Trim(), Email = emailTrimmed, Phone = phone };
        var provision = await _provisioner.ProvisionAsync(associate.Id, emailTrimmed, ct);
        if (!provision.IsSuccess) return Result.Invalid<AssociateResponse>(provision.Errors);
        associate.UserId = provision.Value;

        await _associates.AddAsync(associate, ct);
        foreach (var pid in positionIds.Distinct())
        {
            await _links.AddAsync(new AssociatePosition { AssociateId = associate.Id, PositionId = pid }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return await GetAsync(associate.Id, ct);
    }

    public async Task<Result<AssociateResponse>> UpdateAsync(string id, string name, string? email, string? phone, IReadOnlyList<string> positionIds, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<AssociateResponse>("Name is required.");
        var validation = await ValidatePositionsAsync(positionIds, ct);
        if (!validation.IsSuccess) return Result.Invalid<AssociateResponse>(validation.Errors);

        var associate = await _associates.GetByIdAsync(id, ct);
        if (associate is null) return Result.NotFound<AssociateResponse>("Associate not found.");
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

    public async Task<Result<AssociateResponse>> SetActiveAsync(string id, bool isActive, CancellationToken ct)
    {
        var associate = await _associates.GetByIdAsync(id, ct);
        if (associate is null) return Result.NotFound<AssociateResponse>("Associate not found.");
        associate.IsActive = isActive;
        associate.UpdatedAt = DateTime.UtcNow;
        _associates.Update(associate);
        await _uow.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    private static AssociateResponse MapToResponse(Associate a) =>
        new(
            a.Id,
            a.Name,
            a.Email,
            a.Phone,
            a.UserId,
            a.IsActive,
            a.Positions
                .OrderBy(ap => ap.Position?.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Select(ap => new AssociatePositionInfo(ap.PositionId, ap.Position?.Name ?? string.Empty))
                .ToList(),
            a.CreatedAt,
            a.UpdatedAt);

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
