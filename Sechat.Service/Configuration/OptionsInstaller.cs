using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Settings;

namespace Sechat.Service.Configuration;

public class OptionsInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.Configure<CorsSettings>(webApplicationBuilder.Configuration.GetSection(nameof(CorsSettings)));
        _ = webApplicationBuilder.Services.Configure<CryptographySettings>(webApplicationBuilder.Configuration.GetSection(nameof(CryptographySettings)));
        _ = webApplicationBuilder.Services.Configure<AppSettings>(webApplicationBuilder.Configuration.GetSection(nameof(AppSettings)));
    }
}
