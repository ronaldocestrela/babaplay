using BabaPlay.Application.DTOs;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Persistence abstraction for tenant metadata stored in the Master database.
/// </summary>
public interface ITenantRepository
{
    /// <summary>Returns the tenant matching the slug, or null if not found.</summary>
    Task<TenantInfoDto?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Returns the tenant matching the id, or null if not found.</summary>
    Task<TenantInfoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns true when a tenant with the given slug already exists.</summary>
    Task<bool> ExistsAsync(string slug, CancellationToken ct = default);

    /// <summary>Persists a new tenant record with Pending provisioning status.</summary>
    Task AddAsync(
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
        CancellationToken ct = default);

    /// <summary>Updates provisioning status and (on success) the connection string.</summary>
    Task UpdateProvisioningAsync(
        Guid id,
        ProvisioningStatus status,
        string connectionString,
        CancellationToken ct = default);

    /// <summary>Updates tenant association metadata fields.</summary>
    Task<bool> UpdateAssociationSettingsAsync(
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
        CancellationToken ct = default);
}
