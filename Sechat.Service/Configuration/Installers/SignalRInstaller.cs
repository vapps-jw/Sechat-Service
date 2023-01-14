using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Hubs.Filters;

namespace Sechat.Service.Configuration.Installers;

public class SignalRInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => webApplicationBuilder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = true;
        options.DisableImplicitFromServicesParameters = true;
        options.MaximumReceiveMessageSize = 128 * 1024;
        options.AddFilter<AuthHubFilter>();
    });
}
