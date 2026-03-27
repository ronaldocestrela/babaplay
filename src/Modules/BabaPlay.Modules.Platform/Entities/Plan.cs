using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Platform.Entities;

public class Plan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int? MaxAssociates { get; set; }
}
