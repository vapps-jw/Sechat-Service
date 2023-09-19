using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sechat.Service.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static Sechat.Service.Controllers.UserControllerForms;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class ImagesController : SechatControllerBase
{
    [HttpPost("process-chat-image")]
    public async Task<IActionResult> ProcessChatImage(IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null) return BadRequest("Image not detected");

        await using var stream = new MemoryStream();
        using var imageProcessor = await Image.LoadAsync(image.OpenReadStream(), cancellationToken);

        var maxH = 500;
        var maxW = 300;

        var width = imageProcessor.Width;
        var height = imageProcessor.Height;

        if (imageProcessor.Height > maxH)
        {
            height = maxH;
        }

        if (imageProcessor.Width > maxW)
        {
            width = maxW;
        }

        imageProcessor.Mutate(x =>
            x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Min,
                Size = new Size(width, height)
            })
        );

        await imageProcessor.SaveAsync(stream, new JpegEncoder() { Quality = 80 }, cancellationToken);
        var imageData = Convert.ToBase64String(stream.ToArray());

        return Ok(new ProcessedImageResponse($"data:image/jepg;base64,{imageData}"));
    }
}
