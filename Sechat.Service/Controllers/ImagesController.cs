using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sechat.Service.Configuration;
using Sechat.Service.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class ImagesController : SechatControllerBase
{
    private readonly ImageConversionService _imageConversionService;

    public ImagesController(ImageConversionService imageConversionService) => _imageConversionService = imageConversionService;

    [HttpPost("process-chat-image")]
    public async Task<IActionResult> ProcessChatImage(IFormFile image, CancellationToken cancellationToken)
    {
        return image is null
            ? BadRequest("Image not detected")
            : Ok(await _imageConversionService.PrepareImageAsync(image, cancellationToken));
    }
}
