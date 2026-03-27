namespace BabaPlay.SharedKernel.Security;

public interface IPermissionResolver
{
    Task<IReadOnlyList<string>> GetPermissionNamesForUserAsync(string userId, CancellationToken cancellationToken = default);
}
