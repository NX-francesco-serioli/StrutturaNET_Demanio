using AdSPMdS.DemanioDigitale.Application.Repositories;
using AdSPMdS.DemanioDigitale.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdSPMdS.DemanioDigitale.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistenceDbContext(configuration);
        services.AddPersistenceRepositories();

        return services;
    }

    internal static IServiceCollection AddPersistenceDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found");

        services.AddDbContext<DemanioDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite()));

        return services;
    }

    internal static IServiceCollection AddPersistenceRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEmailOutboxRepository, EmailOutboxRepository>();
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        return services;
    }
}
