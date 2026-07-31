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
            _tenantState.SetTenant(snapshot.TenantId.Value, snapshot.TenantName, snapshot.TenantSlug);
        }
        else
        {
            _tenantState.ClearTenant();
        }

        _authStateProvider.NotifyUserAuthentication(snapshot.AccessToken);
    }

    public async Task ClearSessionAsync(CancellationToken cancellationToken = default)
    {
        _userSessionState.Clear();
        _tenantState.ClearTenant();
        await _storage.ClearAsync(cancellationToken);
        _authStateProvider.NotifyUserLogout();
    }
}
