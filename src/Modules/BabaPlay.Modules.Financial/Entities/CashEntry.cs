using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Financial.Entities;

public class CashEntry : BaseEntity
{
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;

    public Category? Category { get; set; }
}
