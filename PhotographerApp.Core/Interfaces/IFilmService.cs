using PhotographerApp.Core.DTOs;

namespace PhotographerApp.Core.Interfaces;

public interface IFilmService
{
    Task<PaginatedResponse<FilmDto>> GetFilmsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    /// <summary>Admin listing — includes unpublished and draft films, unlike <see cref="GetFilmsAsync"/>.</summary>
    Task<PaginatedResponse<FilmDto>> GetAllFilmsForAdminAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<FilmDetailDto?> GetFilmBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<FilmDto>> GetHeroFilmsAsync(CancellationToken cancellationToken = default);
    Task<List<FilmMapDto>> GetFilmsForMapAsync(CancellationToken cancellationToken = default);
    Task<FilmDto?> CreateFilmAsync(CreateFilmRequest request, CancellationToken cancellationToken = default);
    Task<FilmDto?> UpdateFilmAsync(int id, UpdateFilmRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteFilmAsync(int id, CancellationToken cancellationToken = default);

    // Gallery ("behind the scenes" photos)
    Task<List<FilmGalleryImageDto>> GetGalleryImagesAsync(int filmId, CancellationToken cancellationToken = default);
    Task<FilmGalleryImageDto?> AddGalleryImageAsync(int filmId, string imagePath, CancellationToken cancellationToken = default);
    /// <summary>Returns the deleted image's path (for the caller to remove the physical file), or null if not found.</summary>
    Task<string?> DeleteGalleryImageAsync(int filmId, int imageId, CancellationToken cancellationToken = default);
    Task<bool> ReorderGalleryImagesAsync(int filmId, List<int> orderedImageIds, CancellationToken cancellationToken = default);
}
