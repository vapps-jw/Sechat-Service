using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Services;

namespace Sechat.Service.Configuration.Installers;

public class SignalRInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddSingleton<SignalRConnectionsMonitor>();
        _ = webApplicationBuilder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.DisableImplicitFromServicesParameters = true;
            options.MaximumReceiveMessageSize = 128 * 1024;
            //options.AddFilter<AuthHubFilter>();
        });
    }
}
