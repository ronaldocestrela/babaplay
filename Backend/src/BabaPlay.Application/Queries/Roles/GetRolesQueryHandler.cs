using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Queries.Roles;

public sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, Result<IReadOnlyList<RoleResponse>>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IReadOnlyList<RoleResponse>>> HandleAsync(GetRolesQuery query, CancellationToken ct = default)
    {
        var roles = await _roleRepository.GetAllActiveAsync(ct);
        return Result<IReadOnlyList<RoleResponse>>.Ok(roles.Select(ToResponse).ToList());
    }

    private static RoleResponse ToResponse(Role role) => new(
        role.Id,
        role.TenantId,
        role.Name,
        role.Description,
        role.IsActive,
        role.CreatedAt,
        role.Permissions.Select(x => x.PermissionId).ToList());
}
