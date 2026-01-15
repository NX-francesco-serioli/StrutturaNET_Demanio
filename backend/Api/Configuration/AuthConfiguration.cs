using AdSPMdS.DemanioDigitale.Domain.Auth;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using AdSPMdS.DemanioDigitale.Persistence;
using Microsoft.AspNetCore.Identity;

namespace AdSPMdS.DemanioDigitale.Api.Configuration;

public static class AuthConfiguration
{
    public static IServiceCollection AddIdentityAndAuthorization(this IServiceCollection services)
    {
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<DemanioDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Permissions.ManageUsers, policy =>
                policy.RequireRole(Roles.Admin)
                      .RequireClaim(Permissions.PermissionClaimType, Permissions.ManageUsers));

            options.AddPolicy(Permissions.ViewReports, policy =>
                policy.RequireClaim(Permissions.PermissionClaimType, Permissions.ViewReports));
        });

        return services;
    }
}
