using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Services;

namespace Sechat.Service.Configuration.Installers;

public class NotificationServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => webApplicationBuilder.Services.AddTransient<PushNotificationService>();
}
