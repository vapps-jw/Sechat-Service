﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Services;

namespace Sechat.Service.Configuration;

public class TokenServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => _ = webApplicationBuilder.Services.AddTransient<ITokenService, TokenService>();
}