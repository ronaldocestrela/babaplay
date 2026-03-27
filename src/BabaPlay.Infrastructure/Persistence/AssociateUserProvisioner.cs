using BabaPlay.Modules.Identity;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Security;
using Microsoft.AspNetCore.Identity;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class AssociateUserProvisioner : IAssociateUserProvisioner
{
    private const string AssociateRoleName = "Associate";

    private readonly UserManager<ApplicationUser> _users;

    public AssociateUserProvisioner(UserManager<ApplicationUser> users) => _users = users;

    public async Task<Result<string>> ProvisionAsync(string associateId, string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Invalid<string>("Email is required.");

        var normalized = email.Trim();
        var password = Guid.NewGuid().ToString("N")[..12] + "Aa1!";

        var user = new ApplicationUser
        {
            Email = normalized,
            UserName = normalized,
            UserType = UserType.Associate,
            AssociateId = associateId
        };

        var create = await _users.CreateAsync(user, password);
        if (!create.Succeeded)
            return Result.Invalid<string>(create.Errors.Select(e => e.Description));

        var roleResult = await _users.AddToRoleAsync(user, AssociateRoleName);
        if (!roleResult.Succeeded)
        {
            await _users.DeleteAsync(user);
            return Result.Invalid<string>(roleResult.Errors.Select(e => e.Description));
        }

        return Result.Success(user.Id);
    }
}
