using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sechat.Service.Dtos;
using Sechat.Service.Services;
using Sechat.Tests.Utils;
using SendGrid;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;

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

    [Fact]
    public async Task GetTokenTestAsync()
    {
        using var masterApp = new MockedApi();
        using var client = masterApp.CreateClient();

        var response = await client.PostAsJsonAsync(@"account/login/token", new UserCredentials() { Password = "u1", Username = "u1" });
        var result = await response.Content.ReadAsStringAsync();

        Assert.True(!string.IsNullOrEmpty(result));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Fact]
    public async Task GetAndUseTokenTestAsync()
    {
        using var masterApp = new MockedApi();
        using var client = masterApp.CreateClient();

        var response = await client.PostAsJsonAsync(@"account/login/token", new UserCredentials() { Password = "u1", Username = "u1" });
        var result = await response.Content.ReadAsStringAsync();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result);
        var authorizedResponse = await client.GetAsync(@"status/ping-authorized");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
