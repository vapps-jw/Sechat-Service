﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Sechat.Service.Configuration.Installers;

public class MVCInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddControllers(option =>
        {
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