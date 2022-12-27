using Sechat.Service.Config;

namespace Sechat.Service.Configuration
{
    public class CorsServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration) =>
            services.AddCors(options => options.AddPolicy(AppConstants.CorsPolicies.WebClient, build => build
                .AllowAnyHeader()
                .WithOrigins(origins: configuration.GetValue("CorsSettings:PortalUrl", ""))
                .AllowAnyMethod()
                .AllowCredentials()));
    }
}
