using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Services.CacheServices;

namespace Sechat.Service.Configuration.Installers;

public class CacheInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddSingleton<SignalRCache>();
        _ = webApplicationBuilder.Services.AddSingleton<ContactSuggestionsCache>();
    }
}
