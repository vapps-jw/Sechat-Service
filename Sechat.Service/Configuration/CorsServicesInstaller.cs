﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Utilities;

namespace Sechat.Service.Configuration;

public class CorsServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) =>
        _ = webApplicationBuilder.Services.AddCors(options => options
            .AddPolicy(AppConstants.CorsPolicies.WebClient, build => build
            .WithOrigins(webApplicationBuilder.Configuration.GetValue("CorsSettings:WebAppUrl", ""))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
}
