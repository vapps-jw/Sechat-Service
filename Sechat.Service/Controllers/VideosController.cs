using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sechat.Service.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class VideosController : SechatControllerBase
{
    [HttpPost("process-chat-video")]
    public async Task<IActionResult> ProcessChatVideo(
        [FromServices] VideoConversionService videoConversionService,
        IFormFile video,
        CancellationToken cancellationToken)
    {
        if (video is null) return BadRequest("Video not detected");
        if (video.Length > 52428800)
        {
            return BadRequest("File too large, max 50MB");
        }
        var result = await videoConversionService.PrepareVideoAsync(video, cancellationToken);

        return Ok(result);
    }
}

