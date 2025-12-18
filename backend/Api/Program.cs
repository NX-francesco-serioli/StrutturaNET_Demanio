using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdSPMdS.DemanioDigitale.Api.Contracts;
using AdSPMdS.DemanioDigitale.Application;
using AdSPMdS.DemanioDigitale.Application.Models;
using AdSPMdS.DemanioDigitale.Domain.Auth;
using AdSPMdS.DemanioDigitale.Domain.Entities;
using AdSPMdS.DemanioDigitale.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
builder.Services.Configure<JwtOptions>(jwtSection);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddCors(options =>
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
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
builder.Services.AddOpenApi();

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

app.MapPost("/api/auth/register", async (
    RegisterRequest request,
    UserManager<ApplicationUser> userManager) =>
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

    return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email, user.DisplayName });
}).RequireAuthorization(Permissions.ManageUsers)
  .WithOpenApi();

app.MapPost("/api/auth/login", async (
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

app.MapGet("/api/auth/me", async (
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

app.MapGet("/api/admin/users", (
    UserManager<ApplicationUser> userManager) =>
{
    var users = userManager.Users
        .Select(u => new { u.Id, u.Email, u.DisplayName })
        .ToList();

    return Results.Ok(users);
}).RequireAuthorization(Permissions.ManageUsers)
  .WithOpenApi();

app.MapGet("/api/reports/summary", [Authorize(Policy = Permissions.ViewReports)] () =>
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

app.MapDefaultEndpoints();

app.Run();

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

static async Task<AuthResponse> BuildTokenAsync(
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
