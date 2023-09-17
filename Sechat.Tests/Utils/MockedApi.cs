using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Sechat.Service.Configuration;

namespace Sechat.Tests.Utils;
internal class MockedApi : WebApplicationFactory<Service.Program>
{
    private readonly string _environment;

    public MockedApi(string environment = AppConstants.CustomEnvironment.Test) => _environment = environment;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _ = builder.UseEnvironment(_environment);
        return base.CreateHost(builder);
    }
}