using Microsoft.AspNetCore.Http;

namespace PhotographerApp.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SavePhotoAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);
    Task<string> SavePhotoThumbnailAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);
    Task<(int Width, int Height)> GetImageDimensionsAsync(Stream fileStream, CancellationToken cancellationToken = default);
    Task<string> SaveVideoAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);
    Task<string> SaveVideoThumbnailAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    bool ValidateImageFile(IFormFile file, out string errorMessage);
    bool ValidateVideoFile(IFormFile file, out string errorMessage);
}
