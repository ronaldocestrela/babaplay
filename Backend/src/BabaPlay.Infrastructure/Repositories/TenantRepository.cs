using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>Tenant metadata repository backed by the Master database.</summary>
public sealed class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;

    public TenantRepository(AppDbContext context) => _context = context;

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
    public async Task AddAsync(
        Guid id,
        string name,
        string slug,
        string logoPath,
        string street,
        string number,
        string? neighborhood,
        string city,
        string state,
        string zipCode,
        double associationLatitude,
        double associationLongitude,
        CancellationToken ct = default)
    {
        _context.Tenants.Add(new Tenant
        {
            Id = id,
            Name = name,
            Slug = slug,
            LogoPath = logoPath,
            Street = street,
            Number = number,
            Neighborhood = neighborhood,
            City = city,
            State = state,
            ZipCode = zipCode,
            AssociationLatitude = associationLatitude,
            AssociationLongitude = associationLongitude,
        });
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (entity is null)
            return false;

        _context.Tenants.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAssociationSettingsAsync(
        Guid id,
        string name,
        int playersPerTeam,
        string? logoPath,
        string street,
        string number,
        string? neighborhood,
        string city,
        string state,
        string zipCode,
        double associationLatitude,
        double associationLongitude,
        CancellationToken ct = default)
    {
        var entity = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (entity is null) return false;

        entity.Name = name;
        entity.PlayersPerTeam = playersPerTeam;
        if (!string.IsNullOrWhiteSpace(logoPath))
            entity.LogoPath = logoPath;
        entity.Street = street;
        entity.Number = number;
        entity.Neighborhood = neighborhood;
        entity.City = city;
        entity.State = state;
        entity.ZipCode = zipCode;
        entity.AssociationLatitude = associationLatitude;
        entity.AssociationLongitude = associationLongitude;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    private static TenantInfoDto Map(Tenant t) => new(
        t.Id,
        t.Name,
        t.Slug,
        t.IsActive,
        t.PlayersPerTeam,
        t.LogoPath,
        t.Street,
        t.Number,
        t.Neighborhood,
        t.City,
        t.State,
        t.ZipCode,
        t.AssociationLatitude,
        t.AssociationLongitude);
}
