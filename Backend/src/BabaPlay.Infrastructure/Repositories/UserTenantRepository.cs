using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>Validates user–tenant membership in the Master database.</summary>
public sealed class UserTenantRepository : IUserTenantRepository
{
    private readonly MasterDbContext _context;

    public UserTenantRepository(MasterDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<bool> IsMemberAsync(string userId, Guid tenantId, CancellationToken ct = default)
        => await _context.UserTenants.AnyAsync(ut => ut.UserId == userId && ut.TenantId == tenantId, ct);
}
