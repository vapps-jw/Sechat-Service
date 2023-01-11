using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos.AutoMapperProfiles;
using Sechat.Service.Services;

namespace Sechat.Service.Configuration;

public class DataServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        //if (webApplicationBuilder.Environment.IsDevelopment())
        //{
        //    _ = webApplicationBuilder.Services.AddDbContextFactory<SechatContext>(options =>
        //         options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        //}

        _ = webApplicationBuilder.Services.AddDbContextFactory<SechatContext>(options =>
            options.UseNpgsql(webApplicationBuilder.Configuration.GetConnectionString("Master"),
            serverAction =>
            {
                _ = serverAction.EnableRetryOnFailure(3);
                _ = serverAction.CommandTimeout(20);
            }));

        _ = webApplicationBuilder.Services.AddScoped<ChatRepository>();
        _ = webApplicationBuilder.Services.AddScoped<UserRepository>();

        _ = webApplicationBuilder.Services.AddTransient<IEncryptor, AesEncryptor>();
        _ = webApplicationBuilder.Services.AddTransient<ITokenService, TokenService>();

        _ = webApplicationBuilder.Services.AddAutoMapper(typeof(DefaultProfile));
    }
}
