using Microsoft.AspNetCore.Builder;

namespace Sechat.Service.Configuration.Installers;

public interface IServiceInstaller
{
    void Install(WebApplicationBuilder webApplicationBuilder);
}
