using AdSPMdS.DemanioDigitale.Domain.Entities;

namespace AdSPMdS.DemanioDigitale.Application.Emails;

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(EmailOutboxMessage message, CancellationToken cancellationToken);
}
