namespace Application.Features.Identity.Roles;

public class CreateRoleRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
