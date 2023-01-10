﻿using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Sechat.Tests.Utils;
internal class MockedApi : WebApplicationFactory<Service.Program>
{
    private readonly string _environment;

    public MockedApi(string environment = "Development") => _environment = environment;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _ = builder.UseEnvironment(_environment);
        return base.CreateHost(builder);
    }
}