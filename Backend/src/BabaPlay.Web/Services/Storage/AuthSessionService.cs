using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Services.Http;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Web.Services.Storage;

public sealed class AuthSessionService
{
    private readonly IAuthSessionStorage _storage;
    private readonly UserSessionState _userSessionState;
    private readonly TenantState _tenantState;
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly IAuthApiService _authApiService;

    public AuthSessionService(
        IAuthSessionStorage storage,
        UserSessionState userSessionState,
        TenantState tenantState,
        CustomAuthStateProvider authStateProvider,
        IAuthApiService authApiService)
    {
        _storage = storage;
        _userSessionState = userSessionState;
        _tenantState = tenantState;
        _authStateProvider = authStateProvider;
        _authApiService = authApiService;
    }

    public async Task SaveCurrentSessionAsync(CancellationToken cancellationToken = default)
    {
        if (!_userSessionState.IsAuthenticated || string.IsNullOrWhiteSpace(_userSessionState.JwtToken))
        {
            await _storage.ClearAsync(cancellationToken);
            return;
        }

        var snapshot = new AuthSessionSnapshot
        {
            AccessToken = _userSessionState.JwtToken,
            RefreshToken = _userSessionState.RefreshToken,
            UserId = _userSessionState.UserId,
            Email = _userSessionState.Email,
            FullName = _userSessionState.FullName,
            Roles = _userSessionState.Roles.ToList(),
            TenantId = _tenantState.CurrentTenantId,
            TenantName = _tenantState.CurrentTenantName,
            TenantSlug = _tenantState.CurrentTenantSlug,
            TenantIsOwner = _tenantState.IsOwner,
        };

        await _storage.SaveAsync(snapshot, cancellationToken);
    }

    public async Task RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await _storage.LoadAsync(cancellationToken);
        if (snapshot is null || string.IsNullOrWhiteSpace(snapshot.AccessToken))
        {
            return;
        }

        if (JwtExpirationHelper.IsExpired(snapshot.AccessToken, DateTime.UtcNow))
        {
            if (string.IsNullOrWhiteSpace(snapshot.RefreshToken))
            {
                await ClearSessionAsync(cancellationToken);
                return;
            }

            var refreshed = await _authApiService.RefreshTokenAsync(snapshot.RefreshToken, cancellationToken);
            if (refreshed is null || string.IsNullOrWhiteSpace(refreshed.AccessToken))
            {
                await ClearSessionAsync(cancellationToken);
                return;
            }

            snapshot.AccessToken = refreshed.AccessToken;
            snapshot.RefreshToken = refreshed.RefreshToken;
            await _storage.SaveAsync(snapshot, cancellationToken);
        }

        _userSessionState.RestoreSession(
            snapshot.AccessToken,
            snapshot.RefreshToken,
            snapshot.UserId ?? string.Empty,
            snapshot.Email ?? string.Empty,
            snapshot.FullName ?? snapshot.Email ?? string.Empty,
            snapshot.Roles);

        if (snapshot.TenantId.HasValue && !string.IsNullOrWhiteSpace(snapshot.TenantName))
        {
            _tenantState.SetTenant(snapshot.TenantId.Value, snapshot.TenantName, snapshot.TenantSlug, snapshot.TenantIsOwner);
            await SyncTenantContextFromApiAsync(snapshot, cancellationToken);
        }
        else
        {
            _tenantState.ClearTenant();
        }

        _authStateProvider.NotifyUserAuthentication(snapshot.AccessToken);
    }

    /// <summary>
    /// Loads tenant-scoped RBAC permissions for UI authorization gates.
    /// </summary>
    public async Task SyncTenantPermissionsAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantState.CurrentTenantId.HasValue)
        {
            _tenantState.SetPermissions([]);
            return;
        }

        var permissions = await _authApiService.GetMyPermissionsAsync(cancellationToken);
        _tenantState.SetPermissions(permissions);
    }

    /// <summary>
    /// Refreshes <see cref="TenantState.IsOwner"/> and permissions from the API so UI policies stay correct.
    /// </summary>
    private async Task SyncTenantContextFromApiAsync(
        AuthSessionSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        if (!snapshot.TenantId.HasValue)
        {
            return;
        }

        var profile = await _authApiService.GetMeAsync(cancellationToken);
        var membership = profile?.Tenants?.FirstOrDefault(t => t.Id == snapshot.TenantId.Value);
        if (membership is not null)
        {
            if (_tenantState.IsOwner != membership.IsOwner
                || !string.Equals(_tenantState.CurrentTenantSlug, membership.Slug, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(_tenantState.CurrentTenantName, membership.Name, StringComparison.Ordinal))
            {
                _tenantState.SetTenant(membership.Id, membership.Name, membership.Slug, membership.IsOwner);
                snapshot.TenantName = membership.Name;
                snapshot.TenantSlug = membership.Slug;
                snapshot.TenantIsOwner = membership.IsOwner;
                await _storage.SaveAsync(snapshot, cancellationToken);
            }
        }

        await SyncTenantPermissionsAsync(cancellationToken);
    }

    public async Task ClearSessionAsync(CancellationToken cancellationToken = default)
    {
        _userSessionState.Clear();
        _tenantState.ClearTenant();
        await _storage.ClearAsync(cancellationToken);
        _authStateProvider.NotifyUserLogout();
    }
}
