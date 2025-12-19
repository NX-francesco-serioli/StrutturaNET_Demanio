namespace AdSPMdS.DemanioDigitale.Application.Events;

public record UserRegisteredEvent(
    string UserId,
    string Email,
    string? DisplayName,
    string[] Roles,
    string[] Permissions,
    DateTime OccurredAtUtc);
