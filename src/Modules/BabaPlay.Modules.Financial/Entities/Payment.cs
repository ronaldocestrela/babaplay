using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Financial.Entities;

public class Payment : BaseEntity
{
    public string MembershipId { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Method { get; set; } = "cash";

    public Membership? Membership { get; set; }
}
