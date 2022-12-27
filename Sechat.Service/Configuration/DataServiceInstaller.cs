using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;

namespace Sechat.Service.Configuration
{
    public class DataServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            _ = services.AddDbContext<SechatContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Master"),
                serverAction =>
                {
                    _ = serverAction.EnableRetryOnFailure(3);
                    _ = serverAction.CommandTimeout(20);
                }));
            _ = services.AddDataProtection()
                        .SetApplicationName("vapps")
                        .PersistKeysToDbContext<SechatContext>();
            _ = services.AddIdentity<IdentityUser, IdentityRole>()
                        .AddEntityFrameworkStores<SechatContext>()
                        .AddDefaultTokenProviders();
        }
    }
}
