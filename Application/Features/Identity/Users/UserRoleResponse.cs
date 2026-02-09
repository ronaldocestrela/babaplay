namespace Application.Features.Identity.Users;

public class UserRoleResponse
{
    public required string RoleId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsAssigned { get; set; }
}

public class UserRolesResponse
{
    public List<UserRoleResponse> UserRoles { get; set; } = [];
}