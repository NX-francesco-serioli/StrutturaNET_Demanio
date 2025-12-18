using AdSPMdS.DemanioDigitale.Domain.Auth;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AdSPMdS.DemanioDigitale.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/admin/users", (
            UserManager<ApplicationUser> userManager) =>
        {
            var users = userManager.Users
                .Select(u => new { u.Id, u.Email, u.DisplayName })
                .ToList();

            return Results.Ok(users);
        }).RequireAuthorization(Permissions.ManageUsers)
          .WithOpenApi();

        return routes;
    }
}
