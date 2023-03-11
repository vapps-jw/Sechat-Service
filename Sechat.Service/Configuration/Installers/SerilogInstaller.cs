using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Sechat.Service.Configuration.Installers;

public class SerilogInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        if (webApplicationBuilder.Environment.IsDevelopment())
        {
            _ = webApplicationBuilder.Host.UseSerilog((context, config) => { _ = config.WriteTo.Console(); });
        }

        if (webApplicationBuilder.Environment.IsProduction())
        {
            var logger = new LoggerConfiguration().WriteTo.File(
                    path: "sechat_log.txt",
                    rollOnFileSizeLimit: true,
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10000000)
                .CreateLogger();

            _ = webApplicationBuilder.Services.AddLogging(opt =>
            {
                _ = opt.SetMinimumLevel(LogLevel.Information);
                _ = opt.AddSerilog(logger: logger, dispose: true);
            });
        }
    }
}
