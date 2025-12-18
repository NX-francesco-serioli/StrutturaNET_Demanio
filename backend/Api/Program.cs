using AdSPMdS.DemanioDigitale.Api.Endpoints;
using AdSPMdS.DemanioDigitale.Api.Configuration;
using AdSPMdS.DemanioDigitale.Application;
using AdSPMdS.DemanioDigitale.Domain.Auth;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using AdSPMdS.DemanioDigitale.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
RegisterServices(builder);
ConfigureSwagger(builder.Services);

var app = builder.Build();

await EnsureDatabaseAsync(app.Services, app.Configuration);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

MapApiEndpoints(app);

app.MapDefaultEndpoints();

app.Run();

void MapApiEndpoints(WebApplication webApp)
{
    webApp.MapAuthEndpoints();
    webApp.MapAdminEndpoints();
    webApp.MapReportsEndpoints();
}

void RegisterServices(WebApplicationBuilder appBuilder)
{
    appBuilder.Services.AddApplication();
    appBuilder.Services.AddPersistence(appBuilder.Configuration);
    appBuilder.Services.AddJwtAuthentication(appBuilder.Configuration);

    appBuilder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",
                    "http://localhost:8081")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "AdSP-MdS API",
            Version = "v1"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
    services.AddOpenApi();
}

static async Task EnsureDatabaseAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[] { Roles.Admin, Roles.User })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@local.test";
    var adminPassword = configuration["Seed:AdminPassword"] ?? "Nexus2025!";
    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin is null)
    {
        admin = new ApplicationUser
        {
            Email = adminEmail,
            UserName = adminEmail,
            DisplayName = "Administrator",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(admin, adminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create seed admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(admin, Roles.Admin);
        await userManager.AddClaimAsync(admin, new Claim(Permissions.PermissionClaimType, Permissions.ManageUsers));
        await userManager.AddClaimAsync(admin, new Claim(Permissions.PermissionClaimType, Permissions.ViewReports));
    }
    else
    {
        var roles = await userManager.GetRolesAsync(admin);
        if (!roles.Contains(Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        var claims = await userManager.GetClaimsAsync(admin);
        if (!claims.Any(c => c.Type == Permissions.PermissionClaimType && c.Value == Permissions.ManageUsers))
        {
            await userManager.AddClaimAsync(admin, new Claim(Permissions.PermissionClaimType, Permissions.ManageUsers));
        }

        if (!claims.Any(c => c.Type == Permissions.PermissionClaimType && c.Value == Permissions.ViewReports))
        {
            await userManager.AddClaimAsync(admin, new Claim(Permissions.PermissionClaimType, Permissions.ViewReports));
        }
    }
}
