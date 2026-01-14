using AdSPMdS.DemanioDigitale.Application.Emails;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using AdSPMdS.DemanioDigitale.Persistence;
using Microsoft.EntityFrameworkCore;

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
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var now = DateTime.UtcNow;

        var batch = await dbContext.EmailOutboxMessages
            .Where(m =>
                (m.Status == EmailOutboxStatus.Pending || m.Status == EmailOutboxStatus.Failed) &&
                (m.NextAttemptAtUtc == null || m.NextAttemptAtUtc <= now))
            .OrderBy(m => m.CreatedAtUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var message in batch)
        {
            message.Status = EmailOutboxStatus.Processing;
            message.LastAttemptAtUtc = now;
            message.AttemptCount += 1;
        }

        if (batch.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var message in batch)
        {
            var result = await sender.SendAsync(message, cancellationToken);
            await UpdateStatusAsync(dbContext, message, result, cancellationToken);
        }
    }

    private async Task UpdateStatusAsync(
        ApplicationDbContext dbContext,
        EmailOutboxMessage message,
        EmailSendResult result,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        if (result.Success)
        {
            message.Status = EmailOutboxStatus.Sent;
            message.SentAtUtc = now;
            message.LastError = null;
            message.NextAttemptAtUtc = null;
        }
        else if (result.Skipped)
        {
            message.Status = EmailOutboxStatus.Skipped;
            message.LastError = result.Error;
            message.NextAttemptAtUtc = null;
        }
        else
        {
            var delaySeconds = Math.Min(300, Math.Pow(2, Math.Min(message.AttemptCount, 8)));
            message.Status = EmailOutboxStatus.Failed;
            message.LastError = result.Error;
            message.NextAttemptAtUtc = now.AddSeconds(delaySeconds);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
