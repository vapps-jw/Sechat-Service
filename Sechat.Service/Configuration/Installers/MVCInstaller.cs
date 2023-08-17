using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Sechat.Service.Configuration.Installers;

public class MVCInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddControllers(option =>
        {
            option.CacheProfiles.Add(AppConstants.CaheProfiles.NoCache,
               new CacheProfile()
               {
                   Duration = 0,
                   Location = ResponseCacheLocation.None
               });
        });
    }
}

//context.Response.Headers[HeaderNames.CacheControl] = "max-age=0,no-cache,must-revalidate";
//            context.Response.Headers[HeaderNames.Expires] = "Tue, 01 Jan 1970 00:00:00 GMT";
//            context.Response.Headers[HeaderNames.Pragma] = "no-cache";