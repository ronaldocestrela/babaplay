using BabaPlay.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BabaPlay.Infrastructure.Workers;

public sealed class EmailDispatchWorker : BackgroundService
{
    private readonly IEmailDispatchQueue _queue;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailDispatchWorker> _logger;

    public EmailDispatchWorker(
        IEmailDispatchQueue queue,
        IEmailService emailService,
        ILogger<EmailDispatchWorker> logger)
    {
        _queue = queue;
        _emailService = emailService;
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
                var result = await _emailService.SendAsync(message, stoppingToken);
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
