using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdSPMdS.DemanioDigitale.Api.Options;
using AdSPMdS.DemanioDigitale.Domain.Auth;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AdSPMdS.DemanioDigitale.Application.Events;
using AdSPMdS.DemanioDigitale.Application.Options;
using MassTransit;
using AdSPMdS.DemanioDigitale.Api.Contracts;

namespace AdSPMdS.DemanioDigitale.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/auth/register", async (
            RegisterRequest request,
            UserManager<ApplicationUser> userManager,
            IPublishEndpoint publishEndpoint) =>
        {
            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                DisplayName = request.DisplayName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }

            if (request.Roles is { Length: > 0 })
            {
                await userManager.AddToRolesAsync(user, request.Roles);
            }

            if (request.Permissions is { Length: > 0 })
            {
                foreach (var permission in request.Permissions.Distinct())
                {
                    await userManager.AddClaimAsync(
                        user,
                        new Claim(Permissions.PermissionClaimType, permission));
                }
            }

            var roles = request.Roles ?? Array.Empty<string>();
            var permissions = request.Permissions?.Distinct().ToArray() ?? Array.Empty<string>();

            await publishEndpoint.Publish(new UserRegisteredEvent(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                roles,
                permissions,
                DateTime.UtcNow));

            return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email, user.DisplayName });
        }).RequireAuthorization(Permissions.ManageUsers)
          .WithOpenApi();

        routes.MapPost("/api/auth/login", async (
            LoginRequest request,
            UserManager<ApplicationUser> userManager,
            IOptions<JwtOptions> jwtOptionsAccessor) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Results.BadRequest(new { message = "Invalid credentials" });
            }

            var token = await BuildTokenAsync(user, userManager, jwtOptionsAccessor.Value);
            return Results.Ok(token);
        }).AllowAnonymous()
          .WithOpenApi();

        routes.MapGet("/api/auth/me", async (
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager,
            IOptions<JwtOptions> jwtOptionsAccessor) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound();
            }

            var token = await BuildTokenAsync(user, userManager, jwtOptionsAccessor.Value);
            return Results.Ok(token);
        }).RequireAuthorization()
          .WithOpenApi();

        return routes;
    }

    private static async Task<AuthResponse> BuildTokenAsync(
        ApplicationUser user,
        UserManager<ApplicationUser> userManager,
        JwtOptions jwtOptions)
    {
        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);
        var permissions = claims
            .Where(c => c.Type == Permissions.PermissionClaimType)
            .Select(c => c.Value)
            .Distinct()
            .ToArray();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);

        var jwt = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id)
            }
            .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r)))
            .Concat(permissions.Select(p => new Claim(Permissions.PermissionClaimType, p))),
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new AuthResponse(
            token,
            expiresAt,
            new UserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                roles.ToArray(),
                permissions));
    }
}
