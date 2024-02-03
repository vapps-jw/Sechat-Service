using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Service.Configuration;
using Sechat.Service.Services.HttpClients;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[Authorize(AppConstants.AuthorizationPolicy.ChatPolicy)]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class LinkPreviewController : SechatControllerBase
{
    [HttpPost()]
    public async Task<IActionResult> GetLinkPreview(
        [FromServices] LinkPreviewHttpClient linkPreviewHttpClient,
        [FromBody] LinkPreviewControllerForms.PreviewRequestForm previewRequestForm)
    {
        var linkPreview = await linkPreviewHttpClient.GetLinkPreview(previewRequestForm.Url);
        return Ok(linkPreview);
    }
}

public class LinkPreviewControllerForms
{
    public class PreviewRequestForm
    {
        public string Url { get; set; }
    }

    public class PreviewRequestFormValidation : AbstractValidator<PreviewRequestForm>
    {
        public PreviewRequestFormValidation() => _ = RuleFor(x => x.Url).NotEmpty();
    }
}
