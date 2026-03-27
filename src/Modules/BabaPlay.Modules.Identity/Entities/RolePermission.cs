using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Identity.Entities;

public class RolePermission : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;
    public string PermissionId { get; set; } = string.Empty;

    public virtual ApplicationRole? Role { get; set; }
    public virtual Permission? Permission { get; set; }
}
