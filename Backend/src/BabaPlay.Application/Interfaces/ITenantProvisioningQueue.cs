namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Non-blocking queue for tenant database provisioning jobs.
/// Implementations use an in-process channel; the consumer is TenantProvisioningWorker.
/// </summary>
public interface ITenantProvisioningQueue
{
    /// <summary>Adds tenantId to the provisioning queue.</summary>
    Task EnqueueAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>Waits for and dequeues the next tenantId. Blocks until an item is available.</summary>
    Task<Guid> DequeueAsync(CancellationToken ct = default);
}
