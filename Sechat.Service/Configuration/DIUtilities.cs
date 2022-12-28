using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Reflection;

namespace Sechat.Service.Configuration
{
    public static class DIUtilities
    {
        public static WebApplicationBuilder InstallServices(
            this WebApplicationBuilder webApplicationBuilder,
            IConfiguration configuration,
            params Assembly[] assemblies)
        {
            var serviceInstallers = assemblies
                    .SelectMany(a => a.DefinedTypes)
                    .Where(IsAssignableToType<IServiceInstaller>)
                    .Select(Activator.CreateInstance)
                    .Cast<IServiceInstaller>();

            foreach (var serviceInstaller in serviceInstallers)
            {
                serviceInstaller.Install(webApplicationBuilder, configuration);
            }

            return webApplicationBuilder;

            static bool IsAssignableToType<T>(TypeInfo typeInfo) =>
                typeof(T).IsAssignableFrom(typeInfo) &&
                !typeInfo.IsInterface &&
                !typeInfo.IsAbstract;

        }
    }
}
