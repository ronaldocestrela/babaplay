using Microsoft.AspNetCore.Identity;

namespace BabaPlay.Modules.Identity;

public class ApplicationUser : IdentityUser
{
    public UserType UserType { get; set; } = UserType.Associate;
    public string? AssociateId { get; set; }
}

public enum UserType
{
    PlatformAdmin = 0,
    AssociationStaff = 1,
    Associate = 2
}
