using Microsoft.AspNetCore.Builder;

namespace Sechat.Service.Configuration
{
    public interface IServiceInstaller
    {
        void Install(WebApplicationBuilder webApplicationBuilder);
    }
}
