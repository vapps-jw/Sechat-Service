using Sechat.Service.Services.HttpClients;
using Sechat.Service.Services.HttpClients.PollyPolicies;

namespace Sechat.Tests;
public class LinkPreviewTests
{
    [Fact]
    public async Task LinkPreviewTestAsync()
    {
        var httpClient = new HttpClient();
        var lps = new LinkPreviewHttpClient(httpClient, new BasicHttpClientPolicy());
        var res = await lps.GetLinkPreview("https://visualstudio.microsoft.com/msdn-platforms/");

        Assert.False(string.IsNullOrEmpty(res.Domain));
        Assert.False(string.IsNullOrEmpty(res.Title));
        Assert.False(string.IsNullOrEmpty(res.Description));
        Assert.False(string.IsNullOrEmpty(res.Domain));
    }
}
