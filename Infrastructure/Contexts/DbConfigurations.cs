using Domain.Entities;
using Finbuckle.MultiTenant;
using Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

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

            // EF Core 8 treats List<enum> as a "primitive collection" and uses JsonCollectionReaderWriter
            // to serialize it as JSON. The explicit HasColumnType("nvarchar(500)") + combined
            // HasConversion(converter, comparer) overload is required to fully opt out of that
            // JSON pipeline and store the values as a plain comma-delimited string instead.
            var positionConverter = new ValueConverter<List<Domain.Enums.SoccerPosition>, string>(
                v => string.Join(',', v.Select(p => p.ToString())),
                v => ConvertPositionStringToList(v));

            var positionComparer = new ValueComparer<List<Domain.Enums.SoccerPosition>>(
                (l1, l2) => ReferenceEquals(l1, l2) || (l1 != null && l2 != null && l1.SequenceEqual(l2)),
                l => l == null ? 0 : l.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                l => l == null ? new List<Domain.Enums.SoccerPosition>() : l.ToList());

            builder
                .Property(a => a.Position)
                .IsRequired()
                .HasColumnType("nvarchar(500)")
                .HasConversion(positionConverter, positionComparer);

            // EF Core 8's PrimitiveCollectionConvention adds a "JsonValueReaderWriter"
            // annotation to every List<TEnum> property. At query-compilation time EF
            // checks that annotation and, when present, routes the column through
            // JsonCollectionReaderWriter BEFORE the ValueConverter runs — causing the
            // "invalid start of value" JSON exception for any non-JSON string stored in
            // the column. Removing the annotation forces EF to use only the
            // ValueConverter pipeline (string ↔ List<SoccerPosition>).
            builder.Property(a => a.Position)
                   .Metadata.RemoveAnnotation("JsonValueReaderWriter");

            builder
                .Property(a => a.UserId)
                .IsRequired();

            builder
                .HasIndex(a => a.UserId)
                .IsUnique();
        }

        private static List<Domain.Enums.SoccerPosition> ConvertPositionStringToList(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<Domain.Enums.SoccerPosition>();
            }

            // First, try JSON array (newer format produced by EF Core JSON conversion).
            try
            {
                var list = JsonSerializer.Deserialize<List<Domain.Enums.SoccerPosition>>(value);
                if (list is not null)
                {
                    return list;
                }
            }
            catch
            {
                // ignore and fall back to legacy parsing
            }

            // Legacy: comma/semicolon-separated values (e.g. "GK,RB,CB")
            return [.. value
                .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token.Trim())
                .Where(token => !string.IsNullOrEmpty(token))
                .Select(token => Enum.TryParse<Domain.Enums.SoccerPosition>(token, true, out var pos) ? pos : (Domain.Enums.SoccerPosition?)null)
                .Where(p => p.HasValue)
                .Select(p => p!.Value)];
        }
    }

    internal class DailyCheckInConfig : IEntityTypeConfiguration<DailyCheckIn>
    {
        public void Configure(EntityTypeBuilder<DailyCheckIn> builder)
        {
            builder
                .ToTable("DailyCheckIns", "Academics")
                .IsMultiTenant();

            builder
                .HasIndex(x => new { x.AssociadoId, x.Date })
                .IsUnique();

            builder
                .Property(x => x.Date)
                .IsRequired()
                .HasColumnType("date");

            builder
                .Property(x => x.CheckInAtUtc)
                .IsRequired();
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
