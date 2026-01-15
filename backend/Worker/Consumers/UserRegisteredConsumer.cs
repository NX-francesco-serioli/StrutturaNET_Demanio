using AdSPMdS.DemanioDigitale.Application.Emails;
using AdSPMdS.DemanioDigitale.Application.Events;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using AdSPMdS.DemanioDigitale.Application.Repositories;
using MassTransit;

namespace AdSPMdS.DemanioDigitale.Worker.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailOutboxRepository _outboxRepository;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(
        IEmailOutboxRepository outboxRepository,
        ILogger<UserRegisteredConsumer> logger)
    {
        _outboxRepository = outboxRepository;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        if (string.IsNullOrWhiteSpace(message.Email))
        {
            _logger.LogWarning(
                "Skipping welcome email because recipient is empty (Id: {UserId})",
                message.UserId);
            return Task.CompletedTask;
        }

        var displayName = string.IsNullOrWhiteSpace(message.DisplayName)
            ? message.Email
            : message.DisplayName;

        var outboxMessage = new EmailOutboxMessage
        {
            Id = Guid.NewGuid(),
            To = message.Email,
            Subject = "Benvenuto in Demanio Digitale",
            Body = $"Ciao {displayName},\n\nIl tuo account e' stato creato.\n",
            Status = EmailOutboxStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Queued welcome email for {Email} (Id: {UserId})",
            message.Email,
            message.UserId);

        return _outboxRepository.AddAsync(outboxMessage, context.CancellationToken);
    }
}
