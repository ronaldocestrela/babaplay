using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Web.Services.State;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly UserSessionState _userSessionState;
    private readonly TenantState _tenantState;
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(UserSessionState userSessionState, TenantState tenantState)
    {
        _userSessionState = userSessionState;
        _tenantState = tenantState;
        // AuthorizeView policies (e.g. CommunicationWrite) read TenantState; re-evaluate when tenant changes.
        _userSessionState.OnChange += StateHasChanged;
        _tenantState.OnChange += StateHasChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_userSessionState.IsAuthenticated || string.IsNullOrWhiteSpace(_userSessionState.JwtToken))
        {
            return Task.FromResult(Anonymous);
        }

        var claims = ParseClaimsFromJwt(_userSessionState.JwtToken).ToList();

        if (!string.IsNullOrEmpty(_userSessionState.UserId))
        {
            claims.RemoveAll(c => c.Type == ClaimTypes.NameIdentifier);
            claims.Add(new Claim(ClaimTypes.NameIdentifier, _userSessionState.UserId));
        }

        if (!string.IsNullOrEmpty(_userSessionState.Email) && !claims.Any(c => c.Type == ClaimTypes.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, _userSessionState.Email));
        }

        foreach (var role in _userSessionState.Roles)
        {
            if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(user));
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        _userSessionState.Clear();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private void StateHasChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        if (string.IsNullOrWhiteSpace(jwt)) return claims;

        var parts = jwt.Split('.');
        if (parts.Length < 2) return claims;

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs == null) return claims;

        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        claims.Add(new Claim(MapClaimType(kvp.Key), ClaimValueFromJsonElement(item)));
                    }
                }
                else
                {
                    claims.Add(new Claim(MapClaimType(kvp.Key), ClaimValueFromJsonElement(element)));
                }
            }
            else
            {
                claims.Add(new Claim(MapClaimType(kvp.Key), kvp.Value?.ToString() ?? ""));
            }
        }

        return claims;
    }

    private static string MapClaimType(string key)
    {
        return key.ToLower() switch
        {
            "sub" or "nameidentifier" => ClaimTypes.NameIdentifier,
            "email" => ClaimTypes.Email,
            "role" or "roles" => ClaimTypes.Role,
            "name" => ClaimTypes.Name,
            _ => key
        };
    }

    private static string ClaimValueFromJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
            _ => element.ToString(),
        };
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
    }
}
