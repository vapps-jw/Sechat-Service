using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sechat.Service.Settings;
using System.IO;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class TemporaryFileService
{
    private readonly FileSettings _settings;

    public TemporaryFileService(IOptionsMonitor<FileSettings> optionsMonitor) => _settings = optionsMonitor.CurrentValue;

    public async Task<string> SaveTemporaryFile(IFormFile video, string uniqueId)
    {
        var fileName = string.Concat(uniqueId, Path.GetExtension(video.FileName));
        var savePath = GetSavePath(fileName);

        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        }

        await using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
        {
            await video.CopyToAsync(fileStream);
        }

        return fileName;
    }

    public bool TemporaryFileExists(string fileName)
    {
        var path = GetSavePath(fileName);
        return File.Exists(path);
    }

    public void DeleteTemporaryFile(string fileName)
    {
        var path = GetSavePath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public string GetSavePath(string fileName) => Path.Combine(_settings.WorkingDirectory, fileName);
}
