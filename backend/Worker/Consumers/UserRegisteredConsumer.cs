using System;
using AdSPMdS.DemanioDigitale.Application.Events;
using MassTransit;

namespace AdSPMdS.DemanioDigitale.Worker.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing user registration for {Email} (Id: {UserId}) at {OccurredAtUtc}",
            message.Email,
            message.UserId,
            message.OccurredAtUtc);

        // TODO: trigger email/webhook/etc.

        return Task.CompletedTask;
    }
}
