using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sechat.Data;

namespace Sechat.Tests.Utils;
internal class MockedApi : WebApplicationFactory<Service.Program>
{
    private readonly string _environment;

    public MockedApi(string environment = "Development") => _environment = environment;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _ = builder.UseEnvironment(_environment);
        _ = builder.ConfigureServices(services =>
        {
            _ = services.AddScoped(sp =>
            {
                return new DbContextOptionsBuilder<SechatContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .UseApplicationServiceProvider(sp)
                    .Options;
            });
        });
        return base.CreateHost(builder);
    }
}