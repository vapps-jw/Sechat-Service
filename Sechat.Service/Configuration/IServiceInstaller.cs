using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Sechat.Service.Configuration
{
    public interface IServiceInstaller
    {
        void Install(WebApplicationBuilder webApplicationBuilder, IConfiguration configuration);
    }
}
