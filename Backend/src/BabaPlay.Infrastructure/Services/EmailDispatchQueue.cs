using System.Threading.Channels;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Infrastructure.Services;

public sealed class EmailDispatchQueue : IEmailDispatchQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>(
        new UnboundedChannelOptions { SingleReader = true });

    public async Task EnqueueAsync(EmailMessage message, CancellationToken ct = default)
        => await _channel.Writer.WriteAsync(message, ct);

    public async Task<EmailMessage> DequeueAsync(CancellationToken ct = default)
        => await _channel.Reader.ReadAsync(ct);
}
