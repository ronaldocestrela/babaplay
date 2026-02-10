namespace Application.Features.Identity.Tokens;

public class TokenRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
