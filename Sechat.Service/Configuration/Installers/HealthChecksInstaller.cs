using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data;

namespace Sechat.Service.Configuration.Installers;

public class HealthChecksInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        var configuration = webApplicationBuilder.Configuration;

        webApplicationBuilder.Services
            .AddHealthChecks()
            .AddNpgSql(webApplicationBuilder.Configuration.GetConnectionString("Master"))
            .AddDbContextCheck<SechatContext>();
    }
}