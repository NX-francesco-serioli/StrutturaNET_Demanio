using AdSPMdS.DemanioDigitale.Application.Emails;
using AdSPMdS.DemanioDigitale.Application.Repositories;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdSPMdS.DemanioDigitale.Persistence.Repositories;

public class EmailOutboxRepository : IEmailOutboxRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EmailOutboxRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EmailOutboxMessage message, CancellationToken cancellationToken)
    {
        _dbContext.EmailOutboxMessages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailOutboxMessage>> GetPendingBatchAsync(
        DateTime nowUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await _dbContext.EmailOutboxMessages
            .Where(m =>
                (m.Status == EmailOutboxStatus.Pending || m.Status == EmailOutboxStatus.Failed) &&
                (m.NextAttemptAtUtc == null || m.NextAttemptAtUtc <= nowUtc))
            .OrderBy(m => m.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkProcessingAsync(
        IEnumerable<EmailOutboxMessage> messages,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        foreach (var message in messages)
        {
            message.Status = EmailOutboxStatus.Processing;
            message.LastAttemptAtUtc = nowUtc;
            message.AttemptCount += 1;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        EmailOutboxMessage message,
        EmailSendResult result,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        if (result.Success)
        {
            message.Status = EmailOutboxStatus.Sent;
            message.SentAtUtc = nowUtc;
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
            message.NextAttemptAtUtc = nowUtc.AddSeconds(delaySeconds);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
