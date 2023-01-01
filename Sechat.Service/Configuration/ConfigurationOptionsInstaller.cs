using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Settings;

namespace Sechat.Service.Configuration
{
    public static class ConfigurationOptionsInstaller
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
        {
            _ = services.Configure<CorsSettings>(config.GetSection(nameof(CorsSettings)));

            return services;
        }
    }
}
