using System.Security.Cryptography;
using System.Text;

namespace BabaPlay.Application.Commands.Tenants;

internal static class AssociationInviteToken
{
    public static string GenerateRawToken()
    {
        Span<byte> bytes = stackalloc byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public static string ComputeHash(string rawToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes);
    }
}
