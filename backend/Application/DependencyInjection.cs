using Microsoft.Extensions.DependencyInjection;

namespace AdSPMdS.DemanioDigitale.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services, validators, behaviors, etc.
        return services;
    }
}
