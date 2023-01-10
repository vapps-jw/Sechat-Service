using Sechat.Tests.Utils;
using System.Net;

namespace Sechat.Tests;

public class MockedApiTests
{
    [Fact]
    public async Task PingTest()
    {
        using var masterApp = new MockedApi();
        using var client = masterApp.CreateClient();

        var res = await client.GetAsync(@"status\ping-api");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
