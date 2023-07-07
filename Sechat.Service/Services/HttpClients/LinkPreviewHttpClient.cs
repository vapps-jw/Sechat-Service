using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sechat.Service.Services.HttpClients;

public class LinkPreviewHttpClient
{
    private readonly HttpClient _httpClient;

    public class LinkPreview
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Img { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Favicon { get; set; } = string.Empty;
    }

    public LinkPreviewHttpClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<LinkPreview> GetLinkPreview(string url)
    {
        var result = new LinkPreview();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var htmlString = await _httpClient.GetStringAsync(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlString);

        await Task.WhenAll(new Task[]
        {
            Task.Run(async () => result.Img = await GetImage(htmlDocument)),
            Task.Run(async () => result.Favicon = await GetFavicon(htmlDocument, url)),
            Task.Run(() =>  result.Title = GetTitle(htmlDocument)),
            Task.Run( () => result.Description = GetDescription(htmlDocument)),
            Task.Run( () => result.Domain = GetDomain(htmlDocument)),
        });

        return result;
    }

    private bool IsBase64String(string base64)
    {
        var buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _);
    }

    private async Task<bool> CheckImage(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return false;

        var stream = await response.Content.ReadAsStreamAsync();
        var readByte = new byte[stream.Length];
        _ = stream.Read(readByte);

        var base64String = Convert.ToBase64String(readByte);
        if (!IsBase64String(base64String)) return false;

        var contentHeader = response.Content.Headers.GetValues("Content-Type");
        return contentHeader.Any() || Regex.IsMatch(contentHeader.First(), "image/*", RegexOptions.IgnoreCase);
    }

    private async Task<string> GetImage(HtmlDocument htmlDocument)
    {
        var res = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrEmpty(res) && await CheckImage(res)) return res;

        res = htmlDocument.DocumentNode.SelectSingleNode("//link[@rel='image_src']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrEmpty(res) && await CheckImage(res)) return res;

        res = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']")?.GetAttributeValue("content", null);
        return !string.IsNullOrEmpty(res) && await CheckImage(res) ? res : string.Empty;
    }

    private string GetTitle(HtmlDocument htmlDocument)
    {
        var node = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrEmpty(node)) return node;

        node = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='twitter:title']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrEmpty(node)) return node;

        node = htmlDocument.DocumentNode.SelectSingleNode("//head/title")?.GetAttributeValue("content", null);
        if (!string.IsNullOrEmpty(node)) return node;

        node = htmlDocument.DocumentNode.SelectSingleNode("//title")?.GetAttributeValue("content", null);
        if (!string.IsNullOrEmpty(node)) return node;

        node = htmlDocument.DocumentNode.SelectSingleNode("//h1")?.InnerHtml;
        if (!string.IsNullOrEmpty(node)) return node;

        node = htmlDocument.DocumentNode.SelectSingleNode("//h2")?.InnerHtml;
        return !string.IsNullOrEmpty(node) ? node : string.Empty;
    }

    private string GetDescription(HtmlDocument htmlDocument)
    {
        var node = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:description']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrEmpty(node)) return node;

        node = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrEmpty(node)) return node;

        node = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrEmpty(node)) return node;

        node = htmlDocument.DocumentNode.SelectSingleNode("//p")?.GetAttributeValue("content", null);
        return !string.IsNullOrEmpty(node) ? node : string.Empty;
    }

    private string GetDomain(HtmlDocument htmlDocument)
    {
        var node = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:url']");
        var href = node?.GetAttributeValue("href", string.Empty);
        if (!string.IsNullOrEmpty(href))
        {
            return href.Replace("www.", ""); ;
        }

        node = htmlDocument.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
        href = node?.GetAttributeValue("href", string.Empty);
        if (!string.IsNullOrEmpty(href))
        {
            return href.Replace("www.", ""); ;
        }

        return string.Empty;
    }

    private async Task<string> GetFavicon(HtmlDocument htmlDocument, string url)
    {
        var myUri = new Uri(url);
        var baseUri = myUri.GetLeftPart(UriPartial.Authority);
        var noLinkIcon = $"{baseUri}/favicon.ico";

        if (await CheckImage(noLinkIcon))
        {
            return noLinkIcon;
        }

        var node = htmlDocument.DocumentNode.SelectSingleNode("//link[@rel=icon][@sizes='16x16']");
        if (node is not null)
        {
            var href = node.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrEmpty(href) && await CheckImage(href))
            {
                return href;
            }
        }

        node = htmlDocument.DocumentNode.SelectSingleNode("//link[@rel='shortcut icon'");
        if (node is not null)
        {
            var href = node.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrEmpty(href) && await CheckImage(href))
            {
                return href;
            }
        }

        var nodes = htmlDocument.DocumentNode.SelectNodes("//link[@rel='icon']");
        foreach (var icon in nodes)
        {
            var href = icon.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrEmpty(href) && await CheckImage(href))
            {
                return href;
            }
        }

        nodes = htmlDocument.DocumentNode.SelectNodes("//link[@rel='apple-touch-icon']//link[@rel='apple-touch-icon-precomposed']");
        foreach (var icon in nodes)
        {
            var href = icon.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrEmpty(href) && await CheckImage(href))
            {
                return href;
            }
        }

        return string.Empty;
    }
}
