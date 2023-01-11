using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Sechat.Tests.Utils;
internal static class DataSeed
{
    public static async Task<MockedApi> SeedData(this MockedApi mockedApi)
    {
        using var serviceScope = mockedApi.Services.CreateScope();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        for (var i = 1; i < 11; i++)
        {
            var name = $"u{i}";
            var user = new IdentityUser(name);
            var res = await userManager.CreateAsync(user, name);
            Console.WriteLine($"--> Creating User: {name} - Success: {res.Succeeded}");
        }

        return mockedApi;
    }
}
