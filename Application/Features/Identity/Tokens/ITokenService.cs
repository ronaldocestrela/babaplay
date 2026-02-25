using BabaPlayShared.Library.Models.Requests.Token;
using BabaPlayShared.Library.Models.Responses.Token;

namespace Application.Features.Identity.Tokens;

public interface ITokenService
{
    Task<TokenResponse> LoginAsync(TokenRequest request);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
