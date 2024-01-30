using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Configuration.MVCFilters;

namespace Sechat.Service.Configuration.Installers;

public class EndpointsInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder.NoCache());
        });

        _ = webApplicationBuilder.Services.AddControllers(option =>
        {
            _ = option.Filters.Add<OperationCancelledExceptionFilter>();
            option.CacheProfiles.Add(AppConstants.CacheProfiles.NoStore,
               new CacheProfile()
               {
                   Duration = 0,
                   Location = ResponseCacheLocation.Any,
                   NoStore = true
               });
        });
    }
}