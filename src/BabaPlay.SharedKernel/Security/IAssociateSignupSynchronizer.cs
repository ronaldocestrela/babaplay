using BabaPlay.SharedKernel.Results;

namespace BabaPlay.SharedKernel.Security;

/// <summary>
/// Coordinates associate persistence for users created in signup flows.
/// Implemented in Infrastructure to keep modules decoupled from persistence details.
/// </summary>
public interface IAssociateSignupSynchronizer
{
    Task<Result<string>> CreateAsync(string name, string email, string userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string associateId, CancellationToken cancellationToken = default);
}