using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sechat.Service.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class ImageConversionService
{
    private readonly ILogger<VideoConversionService> _logger;

    public ImageConversionService(ILogger<VideoConversionService> logger) => _logger = logger;

    public record ImageConversionResult(string Data);

    public async Task<ImageConversionResult> PrepareImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        using var imageProcessor = await Image.LoadAsync(image.OpenReadStream(), cancellationToken);

        var maxH = 500;
        var maxW = 320;

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
        var imageData = $"{AppConstants.Files.Base64jpegPrefix}{Convert.ToBase64String(stream.ToArray())}";

        return new ImageConversionResult(imageData);
    }
}