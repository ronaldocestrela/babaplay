using System.Security.Claims;

namespace BabaPlay.SharedKernel.Security;

public interface IAccessTokenIssuer
{
    string Issue(IReadOnlyCollection<Claim> claims);
}
