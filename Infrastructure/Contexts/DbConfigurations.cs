using Domain.Entities;
using Finbuckle.MultiTenant;
using Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Contexts;

internal class DbConfigurations
{
    internal class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder
                .ToTable("Users", "Identity")
                .IsMultiTenant();
        }
    }

    internal class ApplicationRoleConfig : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder
                .ToTable("Roles", "Identity")
                .IsMultiTenant();
        }
    }
    internal class ApplicationRoleClaimConfig : IEntityTypeConfiguration<ApplicationRoleClaim>
    {
        public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder) =>
            builder
                .ToTable("RoleClaims", "Identity")
                .IsMultiTenant();
    }

    internal class IdentityUserRoleConfig : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder) =>
            builder
                .ToTable("UserRoles", "Identity")
                .IsMultiTenant();
    }

    internal class IdentityUserClaimConfig : IEntityTypeConfiguration<IdentityUserClaim<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder) =>
            builder
                .ToTable("UserClaims", "Identity")
                .IsMultiTenant();
    }

    internal class IdentityUserLoginConfig : IEntityTypeConfiguration<IdentityUserLogin<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder) =>
            builder
                .ToTable("UserLogins", "Identity")
                .IsMultiTenant();
    }

    internal class IdentityUserTokenConfig : IEntityTypeConfiguration<IdentityUserToken<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder) =>
            builder
                .ToTable("UserTokens", "Identity")
                .IsMultiTenant();
    }

    internal class AssociationConfig : IEntityTypeConfiguration<Association>
    {
        public void Configure(EntityTypeBuilder<Association> builder)
        {
            builder
                .ToTable("Associations", "Academics")
                .IsMultiTenant();

            builder
                .Property(Association => Association.Name)
                .IsRequired()
                .HasMaxLength(60);
        }
    }

    internal class AssociadoConfig : IEntityTypeConfiguration<Associado>
    {
        public void Configure(EntityTypeBuilder<Associado> builder)
        {
            builder
                .ToTable("Associados", "Academics")
                .IsMultiTenant();

            builder
                .Property(a => a.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder
                .Property(a => a.CPF)
                .IsRequired()
                .HasMaxLength(11);

            builder
                .HasIndex(a => a.CPF);

            builder
                .Property(a => a.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder
                .Property(a => a.Address)
                .IsRequired()
                .HasMaxLength(300);

            builder
                .Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            builder
                .Property(a => a.State)
                .IsRequired()
                .HasMaxLength(2);

            builder
                .Property(a => a.ZipCode)
                .IsRequired()
                .HasMaxLength(10);

            builder
                .Property(a => a.Position)
                .IsRequired()
                .HasMaxLength(50);

            builder
                .Property(a => a.UserId)
                .IsRequired();

            builder
                .HasIndex(a => a.UserId)
                .IsUnique();
        }
    }

    internal class AllowedCorsConfig : IEntityTypeConfiguration<CorsOrigin>
    {
        public void Configure(EntityTypeBuilder<CorsOrigin> builder)
        {
            builder
                .ToTable("CorsOrigins");

            // This configuration is applied only by the *SharedDbContext*, which
            // is pointed at the single shared database defined by
            // DefaultConnection. Tenant-specific contexts intentionally skip this
            // configuration so that the CORS table does **not** appear in each
            // tenant database.

            // NOTE: the entity itself is also not marked multi-tenant; the shared
            // table is common for all tenants.

            builder
                .HasKey(x => x.Id); 
            builder
                .Property(x => x.Origin).IsRequired().HasMaxLength(255); 
            builder
                .HasIndex(x => x.Origin).IsUnique();
        }
    }
}
