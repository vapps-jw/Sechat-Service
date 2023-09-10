using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using Sechat.Data.Models.UserDetails;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class ImageService
{
    private readonly IDbContextFactory<SechatContext> _contextFactory;
    private readonly ILogger<ImageService> _logger;

    public ImageService(
        IDbContextFactory<SechatContext> contextFactory,
        ILogger<ImageService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<int> SaveProfileImage(IFormFile image, string userId, CancellationToken cancellationToken)
    {
        if (image is null || string.IsNullOrEmpty(userId)) return 0;

        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await ctx.UserProfiles
            .Include(p => p.Features)
            .FirstOrDefaultAsync(x => x.Id.Equals(userId));

        await using (var stream = new MemoryStream())
        using (var imageProcessor = await Image.LoadAsync(image.OpenReadStream(), cancellationToken))
        {
            imageProcessor.Mutate(x => x.Resize(48, 48));
            await imageProcessor.SaveAsync(stream, new JpegEncoder());

            var base64 = Convert.ToBase64String(stream.ToArray());

            _ = user.Features.RemoveAll(f => f.Name.Equals(UserFeatures.ProfilePicture));
            user.Features.Add(new Feature()
            {
                Name = UserFeatures.ProfilePicture,
                Value = base64
            });
        }

        return await ctx.SaveChangesAsync();
    }
}
