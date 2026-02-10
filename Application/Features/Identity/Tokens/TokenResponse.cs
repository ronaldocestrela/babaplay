namespace Application.Features.Identity.Tokens;

public class TokenResponse
{
    public required string Jwt { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTime RefreshTokenExpiryDate { get; set; }
}
