using System.Text;
using Vibetech.Educat.Common.Interfaces;

namespace Vibetech.Educat.Domain.Services;

public class FileStorageService : IFileStorageService
{
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();
        return Convert.ToBase64String(bytes);
    }

    public Task<Stream> GetFileAsync(string fileBase64)
    {
        var bytes = Convert.FromBase64String(fileBase64);
        var memoryStream = new MemoryStream(bytes);
        return Task.FromResult<Stream>(memoryStream);
    }

    public Task DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }

    public async Task<string> SavePhotoAsync(Stream photoStream, string fileName)
    {
        using var memoryStream = new MemoryStream();
        await photoStream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();
        return Convert.ToBase64String(bytes);
    }

    public async Task<string> GetPhotoBase64Async(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Photo not found", filePath);

        var bytes = await File.ReadAllBytesAsync(filePath);
        return Convert.ToBase64String(bytes);
    }
} 