using Application.Features.Tenancy;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BabaPlayShared.Library.Constants;
using BabaPlayShared.Library.Models.Requests.Tenancy;
using BabaPlayShared.Library.Models.Responses.Tenency;

namespace Infrastructure.Tenancy;

public class TenantService(IMultiTenantStore<BabaPlayTenantInfo> tenantStore, ApplicationDbSeeder dbSeeder, IServiceProvider serviceProvider) : ITenantService
{
    private readonly IMultiTenantStore<BabaPlayTenantInfo> _tenantStore = tenantStore;
    private readonly ApplicationDbSeeder _dbSeeder = dbSeeder;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<string> ActivateAsync(string id)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(id);
        tenantInDb.IsActive = true;

        await _tenantStore.TryUpdateAsync(tenantInDb);
        return tenantInDb.Identifier;
    }

    public async Task<string> CreateTenantAsync(CreateTenantRequest createTenant, CancellationToken ct)
    {
        var newTenant = new BabaPlayTenantInfo
        {
            Id = createTenant.Identifier,
            Identifier = createTenant.Identifier,
            Name = createTenant.Name,
            IsActive = createTenant.IsActive,
            ConnectionString = createTenant.ConnectionString,
            Email = createTenant.Email,
            FirstName = createTenant.FirstName,
            LastName = createTenant.LastName,
            ValidUpTo = createTenant.ValidUpTo
        };

        await _tenantStore.TryAddAsync(newTenant);

        // Seeding tenant data
        using var scope = _serviceProvider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<BabaPlayTenantInfo> { TenantInfo = newTenant };
        await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
            .InitializeDatabaseAsync(ct);

        return newTenant.Identifier;
    }

    public async Task<string> SignupAsync(SignupRequest request, CancellationToken ct)
    {
        // generate identifier (slug) if not provided
        var identifier = string.IsNullOrWhiteSpace(request.Identifier)
            ? Regex.Replace(request.AssociationName.Trim().ToLowerInvariant(), "\\s+", "-")
            : request.Identifier.Trim();

        // ensure uniqueness of identifier and tenant email
        var existingTenants = await _tenantStore.GetAllAsync();
        if (existingTenants.Any(t => string.Equals(t.Identifier, identifier, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception("Identifier já em uso.");
        }

        if (existingTenants.Any(t => !string.IsNullOrEmpty(t.Email) && string.Equals(t.Email, request.Admin.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception("Email já cadastrado em outro tenant.");
        }

        // build tenant DB connection string from DefaultConnection but with tenant-specific database name
        var defaultConn = _serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection") ?? string.Empty;
        var tenantConnectionString = Regex.Replace(defaultConn, "(Initial Catalog|Database)=([^;]+)", $"Initial Catalog=BabaPlay_{identifier}", RegexOptions.IgnoreCase);
        if (tenantConnectionString == defaultConn)
        {
            tenantConnectionString = defaultConn + $";Initial Catalog=BabaPlay_{identifier}";
        }

        // prepare tenant info
        var nameParts = request.Admin.FullName.Trim().Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : nameParts[0];

        var newTenant = new BabaPlayTenantInfo
        {
            Id = Guid.NewGuid().ToString(),
            Identifier = identifier,
            Name = request.AssociationName,
            IsActive = true,
            ConnectionString = tenantConnectionString,
            Email = request.Admin.Email,
            FirstName = firstName,
            LastName = lastName,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            PhoneNumber = request.PhoneNumber,
            ValidUpTo = DateTime.UtcNow.AddDays(30)
        };

        await _tenantStore.TryAddAsync(newTenant);

        // Ensure tenant database exists (connect to master and create DB if necessary)
        var masterConn = Regex.Replace(defaultConn, "(Initial Catalog|Database)=([^;]+)", "Initial Catalog=master", RegexOptions.IgnoreCase);
        if (masterConn == defaultConn)
        {
            masterConn = defaultConn + ";Initial Catalog=master";
        }

        var tenantDbName = $"BabaPlay_{identifier}";
        try
        {
            await using var masterConnection = new Microsoft.Data.SqlClient.SqlConnection(masterConn);
            await masterConnection.OpenAsync(ct);
            await using var createCmd = masterConnection.CreateCommand();
            createCmd.CommandText = $@"IF DB_ID(N'{tenantDbName}') IS NULL
BEGIN
    CREATE DATABASE [{tenantDbName}];
END";
            await createCmd.ExecuteNonQueryAsync(ct);
        }
        catch (Exception ex)
        {
            // rollback tenantStore add if we failed to create database
            try { await _tenantStore.TryRemoveAsync(newTenant.Id); } catch { }
            throw new Exception("Falha ao criar banco de dados do tenant.", ex);
        }

        // Seeding / migrations: set tenant context, apply migrations, then initialize roles and create admin
        using var scope = _serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<BabaPlayTenantInfo> { TenantInfo = newTenant };

        var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if ((await appDbContext.Database.GetPendingMigrationsAsync(ct)).Any())
        {
            await appDbContext.Database.MigrateAsync(ct);
        }

        // initialize roles now that DB and schema exist
        var seeder = scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>();
        await seeder.InitializeRolesAsync(ct);

        // create admin user with provided password and create associado + association record
        try
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Infrastructure.Identity.Models.ApplicationUser>>();

            if (await userManager.FindByEmailAsync(request.Admin.Email) is not null)
            {
                // rollback tenant creation and db
                try { await _tenantStore.TryRemoveAsync(newTenant.Id); } catch { }
                throw new Exception("Email já está em uso.");
            }

            var adminUser = new Infrastructure.Identity.Models.ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = request.Admin.Email,
                UserName = request.Admin.Email,
                EmailConfirmed = true,
                PhoneNumber = request.Admin.PhoneNumber,
                IsActive = true
            };

            var identityResult = await userManager.CreateAsync(adminUser, request.Admin.Password);
            if (!identityResult.Succeeded)
            {
                try { await _tenantStore.TryRemoveAsync(newTenant.Id); } catch { }
                throw new Application.Exceptions.IdentityException(Identity.IdentityHelper.GetIdentityResultErrorDescriptions(identityResult));
            }

            await userManager.AddToRoleAsync(adminUser, RoleConstants.Admin);

            // create associado record
            var associado = new Domain.Entities.Associado
            {
                FullName = request.Admin.FullName,
                CPF = request.Admin.CPF,
                DateOfBirth = request.Admin.DateOfBirth,
                PhoneNumber = request.Admin.PhoneNumber,
                Address = request.Admin.Address,
                City = request.Admin.City,
                State = request.Admin.State,
                ZipCode = request.Admin.ZipCode,
                Position = request.Admin.Position,
                UserId = adminUser.Id
            };

            await appDbContext.Associados.AddAsync(associado, ct);

            // create a default Association entity in tenant DB
            var association = new Domain.Entities.Association { Name = request.AssociationName, EstablishedDate = DateTime.UtcNow };
            await appDbContext.Associations.AddAsync(association, ct);

            await appDbContext.SaveChangesAsync(ct);

            return newTenant.Identifier;
        }
        catch
        {
            // if something goes wrong, try to remove tenant from store to avoid orphan
            try { await _tenantStore.TryRemoveAsync(newTenant.Id); } catch { }
            throw;
        }
    }

    public async Task<string> DeactivateAsync(string id)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(id);
        tenantInDb.IsActive = false;

        await _tenantStore.TryUpdateAsync(tenantInDb);
        return tenantInDb.Identifier;
    }

    public async Task<TenantResponse> GetTenantByIdAsync(string id)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(id);

        #region Manual Mapping
        //var tenantResponse = new TenantResponse
        //{
        //    Identifier = tenantInDb.Identifier,
        //    Name = tenantInDb.Name,
        //    ConnectionString = tenantInDb.ConnectionString,
        //    Email = tenantInDb.Email,
        //    FirstName = tenantInDb.FirstName,
        //    LastName = tenantInDb.LastName,
        //    IsActive = tenantInDb.IsActive,
        //    ValidUpTo = tenantInDb.ValidUpTo
        //};
        //return tenantResponse;
        #endregion
        // Mapster
        return tenantInDb.Adapt<TenantResponse>();

    }

    public async Task<List<TenantResponse>> GetTenantsAsync()
    {
        var tenantsInDb = await _tenantStore.GetAllAsync();
        return tenantsInDb.Adapt<List<TenantResponse>>();
    }

    public async Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscription)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(updateTenantSubscription.TenantId);

        tenantInDb.ValidUpTo = updateTenantSubscription.NewExpiryDate;

        await _tenantStore.TryUpdateAsync(tenantInDb);

        return tenantInDb.Identifier;
    }
}
