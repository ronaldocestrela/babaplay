using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Financial.Entities;

public class Membership : BaseEntity
{
    public string AssociateId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Pending;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public enum MembershipStatus
{
    Pending = 0,
    Paid = 1,
    Overdue = 2
}
