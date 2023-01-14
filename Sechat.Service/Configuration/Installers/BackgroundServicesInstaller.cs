﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.BackgroundServices;

namespace Sechat.Service.Configuration.Installers;

public class BackgroundServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => webApplicationBuilder.Services.AddHostedService<MessageCleaner>();
}