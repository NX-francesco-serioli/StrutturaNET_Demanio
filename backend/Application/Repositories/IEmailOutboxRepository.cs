using AdSPMdS.DemanioDigitale.Application.Emails;
using AdSPMdS.DemanioDigitale.Domain.Entities;

namespace AdSPMdS.DemanioDigitale.Application.Repositories;

public interface IEmailOutboxRepository
{
    Task AddAsync(EmailOutboxMessage message, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmailOutboxMessage>> GetPendingBatchAsync(
        DateTime nowUtc,
        int batchSize,
        CancellationToken cancellationToken);
    Task MarkProcessingAsync(
        IEnumerable<EmailOutboxMessage> messages,
        DateTime nowUtc,
        CancellationToken cancellationToken);
    Task UpdateStatusAsync(
        EmailOutboxMessage message,
        EmailSendResult result,
        DateTime nowUtc,
        CancellationToken cancellationToken);
}
