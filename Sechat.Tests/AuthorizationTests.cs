using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sechat.Service.Services;
using Sechat.Tests.Utils;
using System.Net;

namespace Sechat.Tests;

public class AuthorizationTests
{
    [Fact]
    public async Task TokenServiceTests()
    {
        using var masterApp = new MockedApi();
        using var scope = masterApp.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();

        var token = await tokenService.GenerateToken("u1");
        var test = await tokenService.ValidateToken("u1", token);

        Assert.True(test);
    }
}
