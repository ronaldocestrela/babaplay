using BabaPlay.SharedKernel.Results;

namespace BabaPlay.SharedKernel.Services;

public interface ITenantProvisioningService
{
    Task<Result> ProvisionDatabaseAsync(string databaseName, string platformConnectionString, CancellationToken cancellationToken = default);
}
