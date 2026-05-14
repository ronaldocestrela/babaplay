using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Services;

public sealed class TenantOwnerProvisioningService : ITenantOwnerProvisioningService
{
    private readonly MasterDbContext _masterDb;
    private readonly UserManager<ApplicationUser> _userManager;

    public TenantOwnerProvisioningService(MasterDbContext masterDb, UserManager<ApplicationUser> userManager)
    {
        _masterDb = masterDb;
        _userManager = userManager;
    }

    public async Task<Result<string>> ResolveOwnerUserIdAsync(
        string? requestedByUserId,
        string? adminEmail,
        string? adminPassword,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(requestedByUserId))
        {
            var existingRequester = await _userManager.FindByIdAsync(requestedByUserId.Trim());
            if (existingRequester is null)
                return Result<string>.Fail("TENANT_OWNER_USER_NOT_FOUND", "Authenticated owner user was not found.");

            if (!existingRequester.IsActive)
                return Result<string>.Fail("TENANT_OWNER_USER_INACTIVE", "Authenticated owner user is inactive.");

            return Result<string>.Ok(existingRequester.Id);
        }

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return Result<string>.Fail(
                "TENANT_ADMIN_CREDENTIALS_REQUIRED",
                "Admin email and password are required to bootstrap the tenant owner.");
        }

        var normalizedEmail = adminEmail.Trim().ToLowerInvariant();
        var password = adminPassword.Trim();

        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is null)
        {
            var newUser = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                EmailConfirmed = true,
                IsActive = true,
            };

            var createResult = await _userManager.CreateAsync(newUser, password);
            if (!createResult.Succeeded)
            {
                var details = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result<string>.Fail("TENANT_ADMIN_USER_CREATE_FAILED", details);
            }

            return Result<string>.Ok(newUser.Id);
        }

        if (!existingUser.IsActive)
            return Result<string>.Fail("TENANT_OWNER_USER_INACTIVE", "Admin user is inactive.");

        var passwordIsValid = await _userManager.CheckPasswordAsync(existingUser, password);
        if (!passwordIsValid)
            return Result<string>.Fail("TENANT_ADMIN_INVALID_PASSWORD", "Provided admin credentials are invalid.");

        return Result<string>.Ok(existingUser.Id);
    }

    public async Task<Result> EnsureOwnerMembershipAsync(string userId, Guid tenantId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Fail("TENANT_OWNER_USER_REQUIRED", "Tenant owner user is required.");

        if (tenantId == Guid.Empty)
            return Result.Fail("TENANT_ID_REQUIRED", "Tenant id is required.");

        var existingMembership = await _masterDb.UserTenants
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId, ct);

        if (existingMembership is null)
        {
            _masterDb.UserTenants.Add(new UserTenant
            {
                UserId = userId,
                TenantId = tenantId,
                IsOwner = true,
                JoinedAt = DateTime.UtcNow,
            });
        }
        else if (!existingMembership.IsOwner)
        {
            existingMembership.IsOwner = true;
        }

        await _masterDb.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
