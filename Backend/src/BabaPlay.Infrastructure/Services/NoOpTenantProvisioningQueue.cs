using BabaPlay.Application.Interfaces;

namespace BabaPlay.Infrastructure.Services;

/// <summary>
/// No-op queue used when tenant database provisioning worker is disabled.
/// </summary>
public sealed class NoOpTenantProvisioningQueue : ITenantProvisioningQueue
{
    public Task EnqueueAsync(Guid tenantId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<Guid> DequeueAsync(CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);
        ct.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
        return tcs.Task;
    }
}
