namespace BabaPlay.Infrastructure.Settings;

public sealed class AssociationInviteSettings
{
    public const string SectionName = "AssociationInvite";

    public string AcceptLinkBaseUrl { get; init; } = "http://localhost:5173/invite/accept";
    public int TokenExpiresInHours { get; init; } = 24;
}
