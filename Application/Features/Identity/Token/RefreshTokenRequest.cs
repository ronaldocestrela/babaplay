namespace Application.Features.Identity.Token;

public class RefreshTokenRequest
{
    public required string CurrentJwt { get; set; }
    public required string CurrentRefreshToken { get; set; }
    public required DateTime RefreshTokenExpiryDate { get; set; }
}
