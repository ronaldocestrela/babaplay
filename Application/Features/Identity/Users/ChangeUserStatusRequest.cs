namespace Application.Features.Identity.Users;

public class ChangeUserStatusRequest
{
    public required string UserId { get; set; }
    public bool Activation { get; set; }
}
