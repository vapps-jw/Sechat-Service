using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Settings;

namespace Sechat.Service.Configuration
{
    public class ConfigurationOptionsInstaller : IServiceInstaller
    {
        public void Install(WebApplicationBuilder webApplicationBuilder) =>
            _ = webApplicationBuilder.Services.Configure<CorsSettings>(webApplicationBuilder.Configuration.GetSection(nameof(CorsSettings)));
    }
}
