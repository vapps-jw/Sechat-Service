using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Settings;

namespace Sechat.Service.Configuration.Installers;

public class OptionsInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.Configure<CorsSettings>(webApplicationBuilder.Configuration.GetSection(nameof(CorsSettings)));
        _ = webApplicationBuilder.Services.Configure<CryptographySettings>(webApplicationBuilder.Configuration.GetSection(nameof(CryptographySettings)));
        _ = webApplicationBuilder.Services.Configure<AppSettings>(webApplicationBuilder.Configuration.GetSection(nameof(AppSettings)));
        _ = webApplicationBuilder.Services.Configure<EmailSenderSettings>(webApplicationBuilder.Configuration.GetSection(nameof(EmailSenderSettings)));
        _ = webApplicationBuilder.Services.Configure<CookieSettings>(webApplicationBuilder.Configuration.GetSection(nameof(CookieSettings)));
        _ = webApplicationBuilder.Services.Configure<VapidKeys>(webApplicationBuilder.Configuration.GetSection(nameof(VapidKeys)));
    }
}
