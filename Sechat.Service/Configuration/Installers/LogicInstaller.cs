using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data.Repositories;

namespace Sechat.Service.Configuration.Installers;

public class LogicInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddScoped<ChatRepository>();
        _ = webApplicationBuilder.Services.AddScoped<UserRepository>();
        _ = webApplicationBuilder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    }
}
