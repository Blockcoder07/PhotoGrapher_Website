using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using PhotographerApp.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace PhotographerApp.Infrastructure.Services.Business;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private const int MaxImageSizeBytes = 10 * 1024 * 1024;
    private const int MaxVideoSizeBytes = 200 * 1024 * 1024;
    private const int ThumbnailWidth = 400;
    // Allow all common image extensions
    private static readonly string[] AllowedImageExtensions =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".ico", ".svg", ".heic", ".heif"
    };
    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".mov", ".avi", ".mkv", ".flv", ".wmv" };
    private static readonly byte[][] ImageMagicNumbers = new[]
    {
        new byte[] { 0xFF, 0xD8, 0xFF },                           // JPEG
        new byte[] { 0x89, 0x50, 0x4E, 0x47 },                     // PNG
        new byte[] { 0x47, 0x49, 0x46, 0x38 },                     // GIF
        new byte[] { 0x42, 0x4D },                                 // BMP
        new byte[] { 0x49, 0x49, 0x2A, 0x00 },                     // TIFF (little-endian)
        new byte[] { 0x4D, 0x4D, 0x00, 0x2A },                     // TIFF (big-endian)
        new byte[] { 0x52, 0x49, 0x46, 0x46 },                     // WEBP
        new byte[] { 0x00, 0x00, 0x01, 0x00 },                     // ICO
        new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 } // HEIC/HEIF
    };
    private static readonly byte[][] VideoMagicNumbers = new[]
    {
        new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 }, // MP4
        new byte[] { 0x1A, 0x45, 0xDF, 0xA3 },                          // MKV
        new byte[] { 0x52, 0x49, 0x46, 0x46 },                          // AVI/WEBM
        new byte[] { 0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70 }   // MOV
    };

    public FileStorageService(string webRootPath = null)
    {
        var uploadsPath = webRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _basePath = Path.Combine(uploadsPath, "uploads");
        EnsureDirectoriesExist();
    }

    public async Task<string> SavePhotoAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("File name cannot be empty", nameof(originalFileName));

        var fileName = GenerateFileName(originalFileName);
        var yearMonth = $"{DateTime.UtcNow:yyyy/MM}";
        var photoDir = Path.Combine(_basePath, "photos", yearMonth);
        Directory.CreateDirectory(photoDir);

        var filePath = Path.Combine(photoDir, fileName);
        using (var fileToWrite = File.Create(filePath))
        {
            await fileStream.CopyToAsync(fileToWrite, cancellationToken);
        }

        return $"/uploads/photos/{yearMonth}/{fileName}";
    }

    public async Task<string> SavePhotoThumbnailAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("File name cannot be empty", nameof(originalFileName));

        var fileName = GenerateFileName(originalFileName);
        var yearMonth = $"{DateTime.UtcNow:yyyy/MM}";
        var photoDir = Path.Combine(_basePath, "photos", yearMonth);
        Directory.CreateDirectory(photoDir);

        var thumbnailFileName = Path.GetFileNameWithoutExtension(fileName) + "_thumb" + Path.GetExtension(fileName);
        var thumbnailPath = Path.Combine(photoDir, thumbnailFileName);

        fileStream.Seek(0, SeekOrigin.Begin);

        using (var image = await Image.LoadAsync(fileStream, cancellationToken))
        {
            var aspectRatio = (double)image.Width / image.Height;
            var newHeight = (int)(ThumbnailWidth / aspectRatio);

            image.Mutate(x => x.Resize(ThumbnailWidth, newHeight, KnownResamplers.Lanczos3));

            await image.SaveAsJpegAsync(thumbnailPath, cancellationToken: cancellationToken);
        }

        return $"/uploads/photos/{yearMonth}/{thumbnailFileName}";
    }

    public async Task<(int Width, int Height)> GetImageDimensionsAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));

        fileStream.Seek(0, SeekOrigin.Begin);
        using (var image = await Image.LoadAsync(fileStream, cancellationToken))
        {
            return (image.Width, image.Height);
        }
    }

    public async Task<string> SaveVideoAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("File name cannot be empty", nameof(originalFileName));

        var fileName = GenerateFileName(originalFileName);
        var yearMonth = $"{DateTime.UtcNow:yyyy/MM}";
        var videoDir = Path.Combine(_basePath, "videos", yearMonth);
        Directory.CreateDirectory(videoDir);

        var filePath = Path.Combine(videoDir, fileName);
        using (var fileToWrite = File.Create(filePath))
        {
            await fileStream.CopyToAsync(fileToWrite, cancellationToken);
        }

        return $"/uploads/videos/{yearMonth}/{fileName}";
    }

    public async Task<string> SaveVideoThumbnailAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("File name cannot be empty", nameof(originalFileName));

        var fileName = GenerateFileName(originalFileName);
        var yearMonth = $"{DateTime.UtcNow:yyyy/MM}";
        var videoDir = Path.Combine(_basePath, "videos", yearMonth);
        Directory.CreateDirectory(videoDir);

        var thumbnailFileName = Path.GetFileNameWithoutExtension(fileName) + "_thumb.jpg";
        var thumbnailPath = Path.Combine(videoDir, thumbnailFileName);

        fileStream.Seek(0, SeekOrigin.Begin);

        try
        {
            using (var image = await Image.LoadAsync(fileStream, cancellationToken))
            {
                var aspectRatio = (double)image.Width / image.Height;
                var newHeight = (int)(ThumbnailWidth / aspectRatio);

                image.Mutate(x => x.Resize(ThumbnailWidth, newHeight, KnownResamplers.Lanczos3));
                await image.SaveAsJpegAsync(thumbnailPath, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Return empty string if thumbnail generation fails
            System.Diagnostics.Debug.WriteLine($"Thumbnail generation failed: {ex.Message}");
            return string.Empty;
        }

        return $"/uploads/videos/{yearMonth}/{thumbnailFileName}";
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        await Task.CompletedTask;
    }

    public bool ValidateImageFile(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (file == null || file.Length == 0)
        {
            errorMessage = "File is empty";
            return false;
        }

        if (file.Length > MaxImageSizeBytes)
        {
            errorMessage = $"File size exceeds {MaxImageSizeBytes / (1024 * 1024)}MB limit";
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
        {
            errorMessage = "File type not allowed. Allowed types: jpg, jpeg, png, gif, bmp, tiff, webp, ico, svg, heic, heif";
            return false;
        }

        if (!ValidateMagicBytes(file, ImageMagicNumbers))
        {
            errorMessage = "File content does not match image format";
            return false;
        }

        return true;
    }

    public bool ValidateVideoFile(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (file == null || file.Length == 0)
        {
            errorMessage = "File is empty";
            return false;
        }

        if (file.Length > MaxVideoSizeBytes)
        {
            errorMessage = $"File size exceeds {MaxVideoSizeBytes / (1024 * 1024)}MB limit";
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedVideoExtensions.Contains(extension))
        {
            errorMessage = "File type not allowed. Allowed types: mp4, webm, mov, avi, mkv, flv, wmv";
            return false;
        }

        if (!ValidateMagicBytes(file, VideoMagicNumbers))
        {
            errorMessage = "File content does not match video format";
            return false;
        }

        return true;
    }

    private bool ValidateMagicBytes(IFormFile file, byte[][] allowedMagicNumbers)
    {
        var buffer = new byte[8];
        file.OpenReadStream().Read(buffer, 0, 8);

        foreach (var magicNumber in allowedMagicNumbers)
        {
            if (buffer.Take(magicNumber.Length).SequenceEqual(magicNumber))
                return true;
        }

        return false;
    }

    private string GenerateFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        return $"{Guid.NewGuid().ToString("N")}{extension}";
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(Path.Combine(_basePath, "photos"));
        Directory.CreateDirectory(Path.Combine(_basePath, "videos"));
        Directory.CreateDirectory(Path.Combine(_basePath, "team"));
    }
}
