using BabaPlay.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Infrastructure.Workers;

public sealed class EmailDispatchWorker : BackgroundService
{
    private readonly IEmailDispatchQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailDispatchWorker> _logger;

    public EmailDispatchWorker(
        IEmailDispatchQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailDispatchWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailDispatchWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            EmailMessage message;
            try
            {
                message = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var result = await emailService.SendAsync(message, stoppingToken);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to dispatch e-mail to {Recipient}. ErrorCode: {ErrorCode}. ErrorMessage: {ErrorMessage}",
                        message.To,
                        result.ErrorCode,
                        result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while dispatching e-mail to {Recipient}", message.To);
            }
        }

        _logger.LogInformation("EmailDispatchWorker stopped.");
    }
}
