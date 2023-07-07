using Sechat.Service.Services.HttpClients;

namespace Sechat.Tests;
public class LinkPreviewTests
{

    [Fact]
    public async Task LinkPreviewTest()
    {
        // https://www.wp.pl/
        // https://visualstudio.microsoft.com/msdn-platforms/

        var httpClient = new HttpClient();
        var lps = new LinkPreviewHttpClient(httpClient);
        var res = await lps.GetLinkPreview("https://visualstudio.microsoft.com/msdn-platforms/");

        Assert.NotNull(res);
    }
}
