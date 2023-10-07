using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sechat.Service.Configuration;
using Sechat.Service.Settings;
using Sechat.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class VideoConversionService
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<VideoConversionService> _logger;
    private readonly TemporaryFileService _temporaryFileService;
    private readonly IOptionsMonitor<FileSettings> _fileSettingsMonitor;

    public VideoConversionService(
        ILogger<VideoConversionService> logger,
        TemporaryFileService temporaryFileService,
        IOptionsMonitor<FileSettings> fileSettingsMonitor)
    {
        _semaphore = new SemaphoreSlim(5);
        _logger = logger;
        _temporaryFileService = temporaryFileService;
        _fileSettingsMonitor = fileSettingsMonitor;
    }

    public record VideoConversionResult(string Thumbnail, string Video);

    public async Task<VideoConversionResult> PrepareVideoAsync(IFormFile video, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(CancellationToken.None);

        var uniqueId = Guid.NewGuid().ToString();

        var inputPath = _temporaryFileService.GetSavePath($"{uniqueId}{Path.GetExtension(video.FileName)}");
        var outputConvertedName = AppConstants.Files.GenerateConvertedFileName(uniqueId);
        var outputThumbnailName = AppConstants.Files.GenerateThumbnailFileName(uniqueId);
        var outputConvertedPath = _temporaryFileService.GetSavePath(outputConvertedName);
        var outputThumbnailPath = _temporaryFileService.GetSavePath(outputThumbnailName);

        try
        {
            _ = _temporaryFileService.SaveTemporaryFile(video, uniqueId);
            var startInfo = new ProcessStartInfo
            {
                FileName = _fileSettingsMonitor.CurrentValue.FFMPEGPath,
                Arguments = $"-y -i {inputPath} -vf scale=320:-1 -f mp4 {outputConvertedPath} -ss 00:00:00 -vframes 1 -vf scale=320:-1 {outputThumbnailPath}",
                //Arguments = $"-y -i {inputPath} -filter:v scale=720:-1 -c:a copy -f mp4 {outputConvertedPath} -ss 00:00:00 -vframes 1 {outputThumbnailPath}",
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                _ = process.Start();
                await process.WaitForExitAsync(cancellationToken);
            }

            if (!_temporaryFileService.TemporaryFileExists(outputConvertedName))
            {
                throw new Exception("FFMPEG failed to generate converted video");
            }

            if (!_temporaryFileService.TemporaryFileExists(outputThumbnailName))
            {
                throw new Exception("FFMPEG failed to generate thumbnail");
            }

            using var videoStream = File.Open(outputConvertedPath, FileMode.Open, FileAccess.Read);
            using var thumbnailStream = File.Open(outputThumbnailPath, FileMode.Open, FileAccess.Read);

            var tasks = new List<Task<string>>
            {
                Task.Run(() => Convert.ToBase64String(videoStream.ToByteArray())),
                Task.Run(() => Convert.ToBase64String(thumbnailStream.ToByteArray()))
            };

            var results = await Task.WhenAll(tasks);

            var videoString = $"{AppConstants.Files.Base64mp4Prefix}{results[0]}";
            var thumbnailString = $"{AppConstants.Files.Base64jpgPrefix}{results[1]}";

            return new VideoConversionResult(thumbnailString, videoString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video conversion failed");
        }
        finally
        {
            _temporaryFileService.DeleteTemporaryFile(inputPath);
            _temporaryFileService.DeleteTemporaryFile(outputConvertedName);
            _temporaryFileService.DeleteTemporaryFile(outputThumbnailName);
            _ = _semaphore.Release();
        }
        return null;
    }
}
