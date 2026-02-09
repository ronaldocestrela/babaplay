namespace Application.Features.Identity.Users;

public class UpdateUserRequest
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PhoneNumber { get; set; }
}
