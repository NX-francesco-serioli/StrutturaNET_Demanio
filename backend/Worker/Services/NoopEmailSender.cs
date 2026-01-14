using AdSPMdS.DemanioDigitale.Application.Emails;
using AdSPMdS.DemanioDigitale.Domain.Entities;

namespace AdSPMdS.DemanioDigitale.Worker.Services;

public class NoopEmailSender : IEmailSender
{
    private readonly ILogger<NoopEmailSender> _logger;

    public NoopEmailSender(ILogger<NoopEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<EmailSendResult> SendAsync(
        EmailOutboxMessage message,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Email provider not configured. Skipping email to {Recipient} (OutboxId: {OutboxId})",
            message.To,
            message.Id);

        return Task.FromResult(new EmailSendResult(false, true, "Email provider not configured"));
    }
}
