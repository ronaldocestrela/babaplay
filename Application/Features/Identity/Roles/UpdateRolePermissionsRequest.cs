namespace Application.Features.Identity.Roles;

public class UpdateRolePermissionsRequest
{
    public required string RoleId { get; set; }
    public required List<string> NewPermissions { get; set; }
}
