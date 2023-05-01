using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

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
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(webApplicationBuilder.Configuration)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: "sechat_log.txt",
                    rollOnFileSizeLimit: true,
                    rollingInterval: RollingInterval.Month,
                    fileSizeLimitBytes: 20971520)
                .CreateLogger();

            _ = webApplicationBuilder.Host.UseSerilog(logger);

            _ = webApplicationBuilder.Services.AddLogging(opt =>
            {
                _ = opt.SetMinimumLevel(LogLevel.Warning);
                _ = opt.AddSerilog(logger: logger, dispose: true);
            });
        }
    }
}
