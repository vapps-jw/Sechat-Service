using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Services;

namespace Sechat.Service.Configuration.Installers;

public class FileServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddSingleton<VideoConversionService>();
        _ = webApplicationBuilder.Services.AddSingleton<TemporaryFileService>();
    }
}
