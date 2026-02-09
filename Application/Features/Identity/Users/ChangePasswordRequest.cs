namespace Application.Features.Identity.Users;

public class ChangePasswordRequest
{
    public required string UserId { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmNewPassword { get; set; }
}
