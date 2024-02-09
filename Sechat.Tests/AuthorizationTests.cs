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
        var test = tokenService.ValidateToken(token);

        Assert.True(test);
    }

    [Fact]
    public void TokenServiceKeyGenerationTests()
    {
        using var masterApp = new MockedApi();
        using var scope = masterApp.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
        var key = tokenService.GenerateSecretKey();

        Assert.True(!string.IsNullOrEmpty(key));
    }
}
