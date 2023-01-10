using Sechat.Service.Dtos;
using Sechat.Tests.Utils;
using System.Net;
using System.Net.Http.Json;

namespace Sechat.Tests;

public class UserActionsTests
{
    [Fact]
    public async Task LoginTest()
    {
        using var masterApp = new MockedApi();
        using var client = masterApp.CreateClient();

        var loginRes = await client.PostAsJsonAsync(@"account\login", new UserCredentials() { Password = "u1", Username = "u1" });
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);

        var secretRes = await client.GetAsync(@"account\test");
        Assert.Equal(HttpStatusCode.OK, secretRes.StatusCode);
    }

    [Fact]
    public async Task FailedLoginTest()
    {
        using var masterApp = new MockedApi();
        using var client = masterApp.CreateClient();

        var loginRes = await client.PostAsJsonAsync(@"account\login", new UserCredentials() { Password = "failed", Username = "failed" });
        Assert.Equal(HttpStatusCode.BadRequest, loginRes.StatusCode);

        var secretRes = await client.GetAsync(@"account\test");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, secretRes.StatusCode);
    }

    [Fact]
    public async Task RegisterTest()
    {
        using var masterApp = new MockedApi();
        using var client = masterApp.CreateClient();

        var loginRes = await client.PostAsJsonAsync(@"account\register", new UserCredentials() { Password = "uNew", Username = "uNew" });
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
    }

    [Fact]
    public async Task FailedRegisterTest()
    {
        using var masterApp = new MockedApi();
        using var client = masterApp.CreateClient();

        var loginRes = await client.PostAsJsonAsync(@"account\register", new UserCredentials() { Password = "u1", Username = "u1" });
        Assert.Equal(HttpStatusCode.BadRequest, loginRes.StatusCode);
    }

    [Fact]
    public async Task ProfileCreationTest()
    {
        using var masterApp = new MockedApi();
        using var client = masterApp.CreateClient();

        var loginRes = await client.PostAsJsonAsync(@"account\login", new UserCredentials() { Password = "u1", Username = "u1" });
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);

        var profile = await client.GetFromJsonAsync<UserProfileProjection>(@"account\get-profile");

        Assert.NotNull(profile);
    }
}
