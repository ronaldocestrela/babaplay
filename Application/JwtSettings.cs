namespace Application;

public class JwtSettings
{
    public required string Secret { get; set; }
    public int TokenExpiryTimeInMinutes { get; set; }
    public int RefreshTokenExpiryTimeInDays { get; set; }
}
