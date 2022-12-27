using System.Reflection;

namespace Sechat.Service.Configuration
{
    public static class DIUtilities
    {
        public static IServiceCollection InstallServices(
            this IServiceCollection services,
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
                serviceInstaller.Install(services, configuration);
            }

            return services;

            static bool IsAssignableToType<T>(TypeInfo typeInfo) =>
                typeof(T).IsAssignableFrom(typeInfo) &&
                !typeInfo.IsInterface &&
                !typeInfo.IsAbstract;

        }
    }
}
