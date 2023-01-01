using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Sechat.Service.Configuration;

public class SignalRInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => webApplicationBuilder.Services.AddSignalR(options =>
    {
        options.DisableImplicitFromServicesParameters = true;
    });
}
