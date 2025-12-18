using AdSPMdS.DemanioDigitale.Domain.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AdSPMdS.DemanioDigitale.Api.Endpoints;

public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/reports/summary", [Authorize(Policy = Permissions.ViewReports)] () =>
        {
            var summary = new
            {
                GeneratedAt = DateTime.UtcNow,
                Items = new[]
                {
                    new { Name = "ActiveUsers", Value = 3 },
                    new { Name = "Admins", Value = 1 }
                }
            };

            return Results.Ok(summary);
        }).WithOpenApi();

        return routes;
    }
}
