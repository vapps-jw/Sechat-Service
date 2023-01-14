﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Sechat.Service.Configuration.Installers;

public class MVCInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => webApplicationBuilder.Services.AddControllers();
}