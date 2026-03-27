using BabaPlay.Modules.Identity.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Identity.Services;

public sealed class RoleAdminService
{
    private readonly RoleManager<ApplicationRole> _roles;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ITenantRepository<Permission> _permissions;

    public RoleAdminService(
        RoleManager<ApplicationRole> roles,
        UserManager<ApplicationUser> users,
        ITenantRepository<Permission> permissions)
    {
        _roles = roles;
        _users = users;
        _permissions = permissions;
    }

    public async Task<Result<IReadOnlyList<string>>> ListRolesAsync(CancellationToken ct)
    {
        var names = await _roles.Roles.Select(r => r.Name!).OrderBy(n => n).ToListAsync(ct);
        return Result.Success<IReadOnlyList<string>>(names);
    }

    public async Task<Result> AssignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return Result.Failure("User not found.", ResultStatus.NotFound);
        if (!await _roles.RoleExistsAsync(roleName)) return Result.Failure("Role not found.", ResultStatus.NotFound);
        var current = await _users.GetRolesAsync(user);
        if (current.Contains(roleName)) return Result.Success();
        var res = await _users.AddToRoleAsync(user, roleName);
        if (!res.Succeeded)
            return Result.Failure(string.Join("; ", res.Errors.Select(e => e.Description)), ResultStatus.Invalid);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<Permission>>> ListPermissionsAsync(CancellationToken ct)
    {
        var list = await _permissions.Query().OrderBy(p => p.Name).ToListAsync(ct);
        return Result.Success<IReadOnlyList<Permission>>(list);
    }
}
