using AdSPMdS.DemanioDigitale.Application.Emails;
using AdSPMdS.DemanioDigitale.Application.Repositories;
using AdSPMdS.DemanioDigitale.Domain.Entities;

namespace AdSPMdS.DemanioDigitale.Worker.Services;

public class EmailOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailOutboxWorker> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

    public EmailOutboxWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailOutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process email outbox batch");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IEmailOutboxRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var now = DateTime.UtcNow;

        var batch = await outboxRepository.GetPendingBatchAsync(now, 10, cancellationToken);
        if (batch.Count > 0)
        {
            await outboxRepository.MarkProcessingAsync(batch, now, cancellationToken);
        }

        foreach (var message in batch)
        {
            var result = await sender.SendAsync(message, cancellationToken);
            await outboxRepository.UpdateStatusAsync(message, result, now, cancellationToken);
        }
    }
}
