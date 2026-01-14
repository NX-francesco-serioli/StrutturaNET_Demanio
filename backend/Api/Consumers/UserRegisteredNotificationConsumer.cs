using AdSPMdS.DemanioDigitale.Api.Hubs;
using AdSPMdS.DemanioDigitale.Api.Notifications;
using AdSPMdS.DemanioDigitale.Application.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace AdSPMdS.DemanioDigitale.Api.Consumers;

public class UserRegisteredNotificationConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IHubContext<NotificationsHub> _hub;
    private readonly ILogger<UserRegisteredNotificationConsumer> _logger;

    public UserRegisteredNotificationConsumer(
        IHubContext<NotificationsHub> hub,
        ILogger<UserRegisteredNotificationConsumer> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        var notification = new NotificationMessage(
            "user-registered",
            $"Nuovo utente registrato: {message.Email}",
            message.OccurredAtUtc,
            message.UserId);

        await _hub.Clients.All.SendAsync("notification", notification, context.CancellationToken);

        _logger.LogInformation(
            "Broadcasted registration notification for user {UserId}",
            message.UserId);
    }
}
