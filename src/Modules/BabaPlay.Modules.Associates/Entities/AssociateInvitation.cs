using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Associates.Entities;

public class AssociateInvitation : BaseEntity
{
    public string? Email { get; set; }
    public bool IsSingleUse { get; set; }
    public string Token { get; set; } = string.Empty;
    public string InvitedByUserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptedByUserId { get; set; }
    public int UsesCount { get; set; }
}
