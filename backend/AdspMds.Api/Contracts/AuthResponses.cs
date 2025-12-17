namespace AdspMds.Api.Contracts;

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    string Id,
    string Email,
    string? DisplayName,
    IEnumerable<string> Roles,
    IEnumerable<string> Permissions);
