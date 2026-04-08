using System.ComponentModel.DataAnnotations;

namespace BabaPlay.Modules.Associates.Services;

public sealed class InvitationLinkOptions
{
    public const string SectionName = "Invitations";

    [Required]
    public string FrontendBaseUrl { get; set; } = string.Empty;
}
