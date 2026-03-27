using BabaPlay.SharedKernel.Security;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class AssociateStatusChecker : IAssociateStatusChecker
{
    private readonly TenantDbContext _db;

    public AssociateStatusChecker(TenantDbContext db) => _db = db;

    public async Task<bool> IsActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return true;

        var associate = await _db.Associates
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (associate is null) return true;
        return associate.IsActive;
    }
}
