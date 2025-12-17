using System.ComponentModel.DataAnnotations;

namespace AdspMds.Api.Contracts;

public record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    string? DisplayName,
    string[]? Roles,
    string[]? Permissions);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password);
