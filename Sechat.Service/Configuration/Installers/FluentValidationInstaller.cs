using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Sechat.Service.Configuration.Installers;

public class FluentValidationInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        if (webApplicationBuilder.Environment.IsProduction())
        {
            _ = webApplicationBuilder.Services.AddFluentValidationAutoValidation();
            _ = webApplicationBuilder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        }
    }
}
