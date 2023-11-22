using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.Services;
using Sechat.Tests.Utils;
using System.Net;

namespace Sechat.Tests;
public class EmailServiceTests
{
    [Fact]
    public async Task ErrorEmailTest()
    {
        using var masterApp = new MockedApi();
        using var scope = masterApp.Services.CreateScope();
        var emailClient = scope.ServiceProvider.GetRequiredService<IEmailClient>();

        try
        {
            throw new Exception("test exception");
        }
        catch (Exception ex)
        {
            var sgResponse = await emailClient.SendExceptionNotificationAsync(ex);
            Assert.Equal(HttpStatusCode.Accepted, sgResponse.StatusCode);
        }
    }
}
