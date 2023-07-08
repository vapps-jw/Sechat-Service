using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Services.HttpClients;
using Sechat.Service.Services.HttpClients.PollyPolicies;

namespace Sechat.Service.Configuration.Installers;

public class HttpServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddHttpClient<LinkPreviewHttpClient>();
        _ = webApplicationBuilder.Services.AddSingleton(new BasicHttpClientPolicy());
    }
}
