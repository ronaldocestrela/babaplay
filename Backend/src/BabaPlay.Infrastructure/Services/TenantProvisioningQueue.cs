using BabaPlay.Application.Interfaces;
using System.Threading.Channels;

namespace BabaPlay.Infrastructure.Services;

/// <summary>
/// In-process channel-based queue for tenant provisioning jobs.
/// Registered as singleton; producer is <see cref="CreateTenantCommandHandler"/>,
/// consumer is <see cref="TenantProvisioningWorker"/>.
/// </summary>
public sealed class TenantProvisioningQueue : ITenantProvisioningQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions { SingleReader = true });

    /// <inheritdoc />
    public async Task EnqueueAsync(Guid tenantId, CancellationToken ct = default)
        => await _channel.Writer.WriteAsync(tenantId, ct);

    /// <inheritdoc />
    public async Task<Guid> DequeueAsync(CancellationToken ct = default)
        => await _channel.Reader.ReadAsync(ct);
}
