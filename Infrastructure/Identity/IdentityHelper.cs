using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

internal static class IdentityHelper
{
    internal static List<string> GetIdentityResultErrorDescriptions(IdentityResult result)
    {
        var errorDescriptions = new List<string>();

        foreach (var error in result.Errors)
        {
            errorDescriptions.Add(error.Description);
        }
        return errorDescriptions;
    }
}
