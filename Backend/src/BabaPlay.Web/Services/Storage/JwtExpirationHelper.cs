using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Web.Services.Storage;

internal static class JwtExpirationHelper
{
    public static bool IsExpired(string jwt, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return true;
        }

        var expClaim = CustomAuthStateProvider.ParseClaimsFromJwt(jwt)
            .FirstOrDefault(c => string.Equals(c.Type, "exp", StringComparison.OrdinalIgnoreCase));

        if (expClaim is null || !long.TryParse(expClaim.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expSeconds))
        {
            return false;
        }

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
        return expiresAt <= utcNow;
    }
}
