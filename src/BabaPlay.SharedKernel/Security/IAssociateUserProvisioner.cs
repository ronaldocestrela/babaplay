using BabaPlay.SharedKernel.Results;

namespace BabaPlay.SharedKernel.Security;

/// <summary>
/// Creates an Identity user for an associate (Associate role, linked to the associate entity id).
/// Implemented in Infrastructure; modules depend only on this contract.
/// </summary>
public interface IAssociateUserProvisioner
{
    Task<Result<string>> ProvisionAsync(string associateId, string email, CancellationToken cancellationToken = default);
}
