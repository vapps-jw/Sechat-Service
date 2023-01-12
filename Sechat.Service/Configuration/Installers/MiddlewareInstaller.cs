using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Middleware;

namespace Sechat.Service.Configuration.Installers;

public class MiddlewareInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();
        _ = webApplicationBuilder.Services.AddTransient<CustomResponseHeadersMiddleware>();
    }
}
