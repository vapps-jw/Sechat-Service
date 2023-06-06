using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.BackgroundServices;
using Sechat.Service.Dtos;
using System.Threading.Channels;

namespace Sechat.Service.Configuration.Installers;

public class BackgroundServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddHostedService<AccountsCleaner>();
        _ = webApplicationBuilder.Services.AddHostedService<MessageCleaner>();
        _ = webApplicationBuilder.Services.AddHostedService<PushNotificationDispatcher>();
        _ = webApplicationBuilder.Services.AddSingleton(Channel.CreateUnbounded<DefaultNotificationDto>());
    }
}
