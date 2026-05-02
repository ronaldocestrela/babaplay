using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>Tenant metadata repository backed by the Master database.</summary>
public sealed class TenantRepository : ITenantRepository
{
    private readonly MasterDbContext _context;

    public TenantRepository(MasterDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<TenantInfoDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var entity = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug, ct);

        return entity is null ? null : Map(entity);
    }

    /// <inheritdoc />
    public async Task<TenantInfoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        return entity is null ? null : Map(entity);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string slug, CancellationToken ct = default)
        => await _context.Tenants.AnyAsync(t => t.Slug == slug, ct);

    /// <inheritdoc />
    public async Task AddAsync(Guid id, string name, string slug, CancellationToken ct = default)
    {
        _context.Tenants.Add(new Tenant
        {
            Id = id,
            Name = name,
            Slug = slug,
            ProvisioningStatus = ProvisioningStatus.Pending,
        });
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateProvisioningAsync(
        Guid id,
        ProvisioningStatus status,
        string connectionString,
        CancellationToken ct = default)
    {
        var entity = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (entity is null) return;

        entity.ProvisioningStatus = status;
        if (!string.IsNullOrWhiteSpace(connectionString))
            entity.ConnectionString = connectionString;

        await _context.SaveChangesAsync(ct);
    }

    private static TenantInfoDto Map(Tenant t) => new(
        t.Id,
        t.Name,
        t.Slug,
        t.IsActive,
        t.ConnectionString,
        t.ProvisioningStatus.ToString());
}
