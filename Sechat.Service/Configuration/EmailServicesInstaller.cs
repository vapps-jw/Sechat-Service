using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Services;

namespace Sechat.Service.Configuration;

public class EmailServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => webApplicationBuilder.Services.AddScoped<IEmailClient, SendGridEmailClient>();
}
