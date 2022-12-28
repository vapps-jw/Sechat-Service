using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Config;

namespace Sechat.Service.Configuration
{
    public class CorsServiceInstaller : IServiceInstaller
    {
        public void Install(WebApplicationBuilder webApplicationBuilder, IConfiguration configuration) =>
            webApplicationBuilder.Services.AddCors(options => options.AddPolicy(AppConstants.CorsPolicies.WebClient, build => build
                .AllowAnyHeader()
                .WithOrigins(origins: configuration.GetValue("CorsSettings:PortalUrl", ""))
                .AllowAnyMethod()
                .AllowCredentials()));
    }
}
