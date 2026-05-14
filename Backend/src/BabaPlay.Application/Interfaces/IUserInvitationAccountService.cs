using BabaPlay.Application.Common;

namespace BabaPlay.Application.Interfaces;

public interface IUserInvitationAccountService
{
    Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken ct = default);
}
