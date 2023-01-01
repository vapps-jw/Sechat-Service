using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Sechat.Service.Configuration
{
    public class SignalRInstaller : IServiceInstaller
    {
        public void Install(WebApplicationBuilder webApplicationBuilder, IConfiguration configuration)
        {
            throw new System.NotImplementedException();
        }

        public void Install(WebApplicationBuilder webApplicationBuilder) => throw new System.NotImplementedException();
    }
}
