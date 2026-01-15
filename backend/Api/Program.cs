using AdSPMdS.DemanioDigitale.Api.Endpoints;
using AdSPMdS.DemanioDigitale.Api.Configuration;
using AdSPMdS.DemanioDigitale.Api.Consumers;
using AdSPMdS.DemanioDigitale.Api.Hubs;
using AdSPMdS.DemanioDigitale.Api.Options;
using AdSPMdS.DemanioDigitale.Api.Storage;
using AdSPMdS.DemanioDigitale.Application;
using AdSPMdS.DemanioDigitale.Application.Options;
using AdSPMdS.DemanioDigitale.Domain.Auth;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using AdSPMdS.DemanioDigitale.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
RegisterServices(builder);

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

app.MapHub<NotificationsHub>("/hubs/notifications").RequireAuthorization();

app.MapDefaultEndpoints();

app.Run();

void MapApiEndpoints(WebApplication webApp)
{
    webApp.MapAuthEndpoints();
    webApp.MapAdminEndpoints();
    webApp.MapReportsEndpoints();
    webApp.MapStorageEndpoints();
}

void RegisterServices(WebApplicationBuilder appBuilder)
{
    appBuilder.Services.AddApplication();
    appBuilder.Services.AddPersistence(appBuilder.Configuration);
    appBuilder.Services.AddIdentityAndAuthorization();
    appBuilder.Services.AddJwtAuthentication(appBuilder.Configuration);
    appBuilder.Services.AddSwaggerDocumentation();
    appBuilder.Services.AddSignalR();
    appBuilder.Services.AddOptions<StorageOptions>()
        .Bind(appBuilder.Configuration.GetSection("Storage"));
    appBuilder.Services.AddSingleton<BlobStorageService>();
    appBuilder.Services.AddOptions<RabbitMqOptions>()
        .Bind(appBuilder.Configuration.GetSection("RabbitMq"))
        .ValidateDataAnnotations();
    appBuilder.Services.AddMassTransit(cfg =>
    {
        cfg.SetKebabCaseEndpointNameFormatter();
        cfg.AddConsumer<UserRegisteredNotificationConsumer>();
        cfg.UsingRabbitMq((context, bus) =>
        {
            var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            bus.Host(options.Host, options.VirtualHost, h =>
            {
                h.Username(options.Username);
                h.Password(options.Password);
            });

            bus.ReceiveEndpoint(options.UserRegisteredNotificationsQueue, endpoint =>
            {
                endpoint.ConfigureConsumer<UserRegisteredNotificationConsumer>(context);
            });
        });
    });

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

static async Task EnsureDatabaseAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DemanioDbContext>();

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
