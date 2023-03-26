using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.BackgroundServices;

namespace Sechat.Service.Configuration.Installers;

public class BackgroundServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddHostedService<AccountsCleaner>();
        _ = webApplicationBuilder.Services.AddHostedService<MessageCleaner>();
    }
}
