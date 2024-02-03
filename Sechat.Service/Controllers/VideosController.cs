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
[Authorize(AppConstants.AuthorizationPolicy.ChatPolicy)]
public class VideosController : SechatControllerBase
{
    private readonly VideoConversionService _videoConversionService;

    public VideosController(VideoConversionService videoConversionService) => _videoConversionService = videoConversionService;

    [HttpPost("process-chat-video")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> ProcessChatVideo(
        IFormFile video,
        CancellationToken cancellationToken)
    {
        if (video is null) return BadRequest("Video not detected");
        if (video.Length > 83886080)
        {
            return BadRequest("File too large, max 80MB");
        }

        var result = await _videoConversionService.PrepareVideoAsync(video, cancellationToken);
        return result is null ? Problem("Video conversion error, try again") : (IActionResult)Ok(result);
    }
}

