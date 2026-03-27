using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Identity.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
