using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;

namespace Sechat.Service.Configuration.Installers;

public class FluentValidationInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddFluentValidationAutoValidation();
        _ = webApplicationBuilder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    }
}
