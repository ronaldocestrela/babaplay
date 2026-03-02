using Finbuckle.MultiTenant.Abstractions;
using BabaPlayShared.Library.Constants;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Application.Features.Cors.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public class ApplicationDbSeeder(
        IMultiTenantContextAccessor<BabaPlayTenantInfo> tenantInfoContextAccessor,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext applicationDbContext,
        SharedDbContext sharedDbContext)
{
    private readonly IMultiTenantContextAccessor<BabaPlayTenantInfo> _tenantInfoContextAccessor = tenantInfoContextAccessor;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
    private readonly SharedDbContext _sharedDbContext = sharedDbContext;

    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        // ensure tenant database exists & is migrated
        if (_applicationDbContext.Database.GetMigrations().Any())
        {
            if ((await _applicationDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await _applicationDbContext.Database.MigrateAsync(cancellationToken);
            }

            if (await _applicationDbContext.Database.CanConnectAsync(cancellationToken))
            {
                await InitializeDefaultRolesAsync(cancellationToken);
                await InitializeAdminUserAsync();
            }
        }

        // always ensure shared database is ready; it uses the default
        // connection string and holds the global CORS list.  migrations are
        // applied once regardless of tenant state.
        if (_sharedDbContext.Database.GetMigrations().Any())
        {
            if ((await _sharedDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await _sharedDbContext.Database.MigrateAsync(cancellationToken);
            }

            if (await _sharedDbContext.Database.CanConnectAsync(cancellationToken))
            {
                await InitializeDefaultCorsOriginsAsync(cancellationToken);
            }
        }
    }

    // Exposed method to initialize only roles (used by public signup flow)
    public async Task InitializeRolesAsync(CancellationToken ct)
    {
        await InitializeDefaultRolesAsync(ct);
    }

    private async Task InitializeDefaultRolesAsync(CancellationToken ct)
    {
        foreach (var roleName in RoleConstants.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(role => role.Name == roleName, ct) is not ApplicationRole incomingRole)
            {
                incomingRole = new ApplicationRole()
                {
                    Name = roleName,
                    Description = $"{roleName} Role"
                };

                await _roleManager.CreateAsync(incomingRole);
            }

            if (roleName == RoleConstants.Admin)
            {
                // Assign Admin Permissions
                await AssignPermissionsToRoleAsync(AssociationPermissions.Admin, incomingRole, ct);

                // ensure CORS origins management is available even though the shared library
                // doesn't yet include this feature constant (temporary local support).
                var corsActions = new[] { AssociationAction.Create, AssociationAction.Read,
                    AssociationAction.Update, AssociationAction.Delete };
                var currentlyAssignedClaims = await _roleManager.GetClaimsAsync(incomingRole);
                foreach (var act in corsActions)
                {
                    var permName = AssociationPermission.NameFor(act, Application.Features.Cors.Constants.CorsFeature.CorsOrigins);
                    if (!currentlyAssignedClaims.Any(claim => claim.Type == ClaimConstants.Permission && claim.Value == permName))
                    {
                        await _applicationDbContext.RoleClaims.AddAsync(new ApplicationRoleClaim
                        {
                            RoleId = incomingRole.Id,
                            ClaimType = ClaimConstants.Permission,
                            ClaimValue = permName,
                            Description = permName,
                            Group = CorsFeature.CorsOrigins
                        }, ct);
                        await _applicationDbContext.SaveChangesAsync(ct);
                    }
                }

                if (_tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Id == TenancyConstants.Root.Id)
                {
                    await AssignPermissionsToRoleAsync(AssociationPermissions.Root, incomingRole, ct);
                }
            }
            else if (roleName == RoleConstants.Basic)
            {
                // Assign Basic Permissions
                await AssignPermissionsToRoleAsync(AssociationPermissions.Basic, incomingRole, ct);
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(
        IReadOnlyList<AssociationPermission> incomingRolePermissions,
        ApplicationRole currentRole,
        CancellationToken ct)
    {
        var currentlyAssignedClaims = await _roleManager.GetClaimsAsync(currentRole);

        foreach (var incomingPermission in incomingRolePermissions)
        {
            if (!currentlyAssignedClaims.Any(claim => claim.Type == ClaimConstants.Permission && claim.Value == incomingPermission.Name))
            {
                await _applicationDbContext.RoleClaims.AddAsync(new ApplicationRoleClaim
                {
                    RoleId = currentRole.Id,
                    ClaimType = ClaimConstants.Permission,
                    ClaimValue = incomingPermission.Name,
                    Description = incomingPermission.Description,
                    Group = incomingPermission.Group
                }, ct);

                await _applicationDbContext.SaveChangesAsync(ct);
            }
        }
    }

    private async Task InitializeDefaultCorsOriginsAsync(CancellationToken ct)
    {
        // ensure at least one entry exists in shared table for development
        if (!await _sharedDbContext.CorsOrigins.AnyAsync(ct))
        {
            _sharedDbContext.CorsOrigins.Add(new CorsOrigin
            {
                Origin = "http://localhost:5145",
                IsActive = true
            });
            await _sharedDbContext.SaveChangesAsync(ct);
        }
    }

    private async Task InitializeAdminUserAsync()
    {
        if (string.IsNullOrEmpty(_tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email)) return;

        if (await _userManager.Users
            .SingleOrDefaultAsync(user => user.Email == _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email)
            is not ApplicationUser incomingUser)
        {
            incomingUser = new ApplicationUser
            {
                FirstName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.FirstName,
                LastName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.LastName,
                Email = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email,
                UserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpperInvariant(),
                NormalizedUserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpperInvariant(),
                IsActive = true,
            };

            var passwordHash = new PasswordHasher<ApplicationUser>();

            incomingUser.PasswordHash = passwordHash.HashPassword(incomingUser, TenancyConstants.DefaultPassword);
            await _userManager.CreateAsync(incomingUser);
        }

        if (!await _userManager.IsInRoleAsync(incomingUser, RoleConstants.Admin))
        {
            await _userManager.AddToRoleAsync(incomingUser, RoleConstants.Admin);
        }
    }
}
