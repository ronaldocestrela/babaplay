using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Services.State;

public class UserSessionState
{
    public string? JwtToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? UserId { get; private set; }
    public string? Email { get; private set; }
    public string? FullName { get; private set; }
    public List<string> Roles { get; private set; } = new();

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(JwtToken);

    public event Action? OnChange;

    public void SetUserSession(
        string jwtToken,
        string userId,
        string email,
        string fullName,
        IEnumerable<string>? roles = null,
        string? refreshToken = null)
    {
        JwtToken = jwtToken;
        RefreshToken = refreshToken;
        UserId = userId;
        Email = email;
        FullName = fullName;
        Roles = roles != null ? new List<string>(roles) : new List<string>();

        NotifyStateChanged();
    }

    public void RestoreSession(
        string jwtToken,
        string? refreshToken,
        string userId,
        string email,
        string fullName,
        IEnumerable<string>? roles = null)
    {
        JwtToken = jwtToken;
        RefreshToken = refreshToken;
        UserId = userId;
        Email = email;
        FullName = fullName;
        Roles = roles != null ? new List<string>(roles) : new List<string>();

        NotifyStateChanged();
    }

    public void Clear()
    {
        JwtToken = null;
        RefreshToken = null;
        UserId = null;
        Email = null;
        FullName = null;
        Roles.Clear();

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
