using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Sechat.Service.Configuration
{
    public class FluentValidationInstaller : IServiceInstaller
    {
        public void Install(WebApplicationBuilder webApplicationBuilder, IConfiguration configuration)
        {
            _ = webApplicationBuilder.Services.AddFluentValidationAutoValidation();
            _ = webApplicationBuilder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        }
    }
}
