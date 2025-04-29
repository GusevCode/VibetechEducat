using Vibetech.Educat.Common.Models;

namespace Vibetech.Educat.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> GetFileAsync(string fileBase64);
    Task DeleteFileAsync(string filePath);
    Task<string> SavePhotoAsync(Stream photoStream, string fileName);
    Task<string> GetPhotoBase64Async(string filePath);
} 