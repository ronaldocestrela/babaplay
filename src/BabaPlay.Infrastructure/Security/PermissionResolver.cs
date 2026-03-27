using BabaPlay.Infrastructure.Persistence;
using BabaPlay.SharedKernel.Security;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Security;

public sealed class PermissionResolver : IPermissionResolver
{
    private readonly TenantDbContext _db;

    public PermissionResolver(TenantDbContext db) => _db = db;

    public async Task<IReadOnlyList<string>> GetPermissionNamesForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var names = await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            join rp in _db.RolePermissions on r.Id equals rp.RoleId
            join p in _db.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId
            select p.Name).Distinct().ToListAsync(cancellationToken);

        return names;
    }
}
