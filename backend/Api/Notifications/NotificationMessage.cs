namespace AdSPMdS.DemanioDigitale.Api.Notifications;

public record NotificationMessage(
    string Type,
    string Message,
    DateTime OccurredAtUtc,
    string? UserId);
