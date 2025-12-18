using Microsoft.Extensions.DependencyInjection;

namespace AdSPMdS.DemanioDigitale.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // Register persistence services (DbContext, repositories, migrations, etc.).
        return services;
    }
}
