using Microsoft.EntityFrameworkCore;
using PhotographerApp.Core.DTOs;
using PhotographerApp.Core.Entities;
using PhotographerApp.Core.Interfaces;
using PhotographerApp.Infrastructure.Data;
using System.Text.RegularExpressions;

namespace PhotographerApp.Infrastructure.Services.Content;

public class FilmService : IFilmService
{
    private readonly ApplicationDbContext _context;

    public FilmService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PaginatedResponse<FilmDto>> GetFilmsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        GetFilmsAsync(_context.Films.Where(f => f.IsPublished), pageNumber, pageSize, cancellationToken);

    public Task<PaginatedResponse<FilmDto>> GetAllFilmsForAdminAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        GetFilmsAsync(_context.Films, pageNumber, pageSize, cancellationToken);

    private static async Task<PaginatedResponse<FilmDto>> GetFilmsAsync(IQueryable<Film> query, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var films = await query
            .OrderBy(f => f.DisplayOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var filmDtos = films.Select(MapToFilmDto).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<FilmDto>
        {
            Items = filmDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<FilmDetailDto?> GetFilmBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var film = await _context.Films
            .Include(f => f.GalleryImages)
            .FirstOrDefaultAsync(f => f.Slug == slug && f.IsPublished, cancellationToken);

        if (film == null)
            return null;

        film.ViewCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return MapToFilmDetailDto(film);
    }

    public async Task<List<FilmDto>> GetHeroFilmsAsync(CancellationToken cancellationToken = default)
    {
        var films = await _context.Films
            .Where(f => f.IsFeaturedHero && f.IsPublished)
            .OrderBy(f => f.DisplayOrder)
            .Take(3)
            .ToListAsync(cancellationToken);

        return films.Select(MapToFilmDto).ToList();
    }

    public async Task<List<FilmMapDto>> GetFilmsForMapAsync(CancellationToken cancellationToken = default)
    {
        var films = await _context.Films
            .Where(f => f.IsPublished)
            .Select(f => new FilmMapDto
            {
                Id = f.Id,
                Slug = f.Slug,
                CoupleNames = f.CoupleNames,
                ShootLocation = f.ShootLocation,
                CountryName = f.CountryName,
                Latitude = f.Latitude,
                Longitude = f.Longitude,
                PosterImagePath = f.PosterImagePath,
                PaletteJson = f.PaletteJson
            })
            .ToListAsync(cancellationToken);

        return films;
    }

    public async Task<FilmDto?> CreateFilmAsync(CreateFilmRequest request, CancellationToken cancellationToken = default)
    {
        // Validation
        if (request.Latitude < -90 || request.Latitude > 90 || request.Longitude < -180 || request.Longitude > 180)
            throw new InvalidOperationException("Invalid coordinates");

        if (request.IsFeaturedHero && string.IsNullOrEmpty(request.HeroVideoUrl))
            throw new InvalidOperationException("Hero films must have a hero video URL");

        var slug = GenerateSlug(request.Title);

        // Check slug uniqueness
        if (await _context.Films.AnyAsync(f => f.Slug == slug, cancellationToken))
            throw new InvalidOperationException("A film with this title already exists");

        var film = new Film
        {
            Title = request.Title,
            Slug = slug,
            CoupleNames = request.CoupleNames,
            ShootLocation = request.ShootLocation,
            CountryName = request.CountryName,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ShootDate = request.ShootDate,
            Story = request.Story,
            Quote = request.Quote,
            HeroVideoUrl = request.HeroVideoUrl,
            TrailerUrl = request.TrailerUrl,
            VideoType = request.VideoType,
            EmbedId = request.EmbedId,
            PosterImagePath = request.PosterImagePath,
            WebpPath = request.WebpPath,
            ThumbnailPath = request.ThumbnailPath,
            BlurHash = request.BlurHash,
            LqipDataUri = request.LqipDataUri,
            PaletteJson = request.PaletteJson,
            DurationSeconds = request.DurationSeconds,
            IsFeaturedHero = request.IsFeaturedHero,
            IsPublished = request.IsPublished,
            DisplayOrder = request.DisplayOrder
        };

        _context.Films.Add(film);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToFilmDto(film);
    }

    public async Task<FilmDto?> UpdateFilmAsync(int id, UpdateFilmRequest request, CancellationToken cancellationToken = default)
    {
        var film = await _context.Films
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (film == null)
            return null;

        if (request.IsFeaturedHero && string.IsNullOrEmpty(request.HeroVideoUrl))
            throw new InvalidOperationException("Hero films must have a hero video URL");

        film.Title = request.Title;
        film.CoupleNames = request.CoupleNames;
        film.ShootLocation = request.ShootLocation;
        film.CountryName = request.CountryName;
        film.Latitude = request.Latitude;
        film.Longitude = request.Longitude;
        film.ShootDate = request.ShootDate;
        film.Story = request.Story;
        film.HeroVideoUrl = request.HeroVideoUrl;
        film.TrailerUrl = request.TrailerUrl;
        film.VideoType = request.VideoType;
        film.EmbedId = request.EmbedId;
        film.PosterImagePath = request.PosterImagePath;
        film.WebpPath = request.WebpPath;
        film.ThumbnailPath = request.ThumbnailPath;
        film.BlurHash = request.BlurHash;
        film.DurationSeconds = request.DurationSeconds;
        film.IsFeaturedHero = request.IsFeaturedHero;
        film.IsPublished = request.IsPublished;
        film.DisplayOrder = request.DisplayOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return MapToFilmDto(film);
    }

    public async Task<bool> DeleteFilmAsync(int id, CancellationToken cancellationToken = default)
    {
        var film = await _context.Films.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (film == null)
            return false;

        film.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<FilmGalleryImageDto>> GetGalleryImagesAsync(int filmId, CancellationToken cancellationToken = default)
    {
        return await _context.FilmGalleryImages
            .Where(g => g.FilmId == filmId)
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new FilmGalleryImageDto { Id = g.Id, ImagePath = g.ImagePath, DisplayOrder = g.DisplayOrder })
            .ToListAsync(cancellationToken);
    }

    public async Task<FilmGalleryImageDto?> AddGalleryImageAsync(int filmId, string imagePath, CancellationToken cancellationToken = default)
    {
        var filmExists = await _context.Films.AnyAsync(f => f.Id == filmId, cancellationToken);
        if (!filmExists)
            return null;

        var maxOrder = await _context.FilmGalleryImages
            .Where(g => g.FilmId == filmId)
            .Select(g => (int?)g.DisplayOrder)
            .MaxAsync(cancellationToken) ?? -1;

        var image = new FilmGalleryImage { FilmId = filmId, ImagePath = imagePath, DisplayOrder = maxOrder + 1 };
        _context.FilmGalleryImages.Add(image);
        await _context.SaveChangesAsync(cancellationToken);

        return new FilmGalleryImageDto { Id = image.Id, ImagePath = image.ImagePath, DisplayOrder = image.DisplayOrder };
    }

    public async Task<string?> DeleteGalleryImageAsync(int filmId, int imageId, CancellationToken cancellationToken = default)
    {
        var image = await _context.FilmGalleryImages
            .FirstOrDefaultAsync(g => g.Id == imageId && g.FilmId == filmId, cancellationToken);
        if (image == null)
            return null;

        var imagePath = image.ImagePath;
        _context.FilmGalleryImages.Remove(image);
        await _context.SaveChangesAsync(cancellationToken);
        return imagePath;
    }

    public async Task<bool> ReorderGalleryImagesAsync(int filmId, List<int> orderedImageIds, CancellationToken cancellationToken = default)
    {
        var images = await _context.FilmGalleryImages
            .Where(g => g.FilmId == filmId)
            .ToListAsync(cancellationToken);

        var byId = images.ToDictionary(g => g.Id);
        for (var i = 0; i < orderedImageIds.Count; i++)
        {
            if (byId.TryGetValue(orderedImageIds[i], out var image))
                image.DisplayOrder = i;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static FilmDto MapToFilmDto(Film film)
    {
        return new FilmDto
        {
            Id = film.Id,
            Title = film.Title,
            Slug = film.Slug,
            CoupleNames = film.CoupleNames,
            ShootLocation = film.ShootLocation,
            CountryName = film.CountryName,
            Latitude = film.Latitude,
            Longitude = film.Longitude,
            ShootDate = film.ShootDate,
            Story = film.Story,
            HeroVideoUrl = film.HeroVideoUrl,
            TrailerUrl = film.TrailerUrl,
            VideoType = film.VideoType,
            EmbedId = film.EmbedId,
            PosterImagePath = film.PosterImagePath,
            ThumbnailPath = film.ThumbnailPath,
            BlurHash = film.BlurHash,
            DurationSeconds = film.DurationSeconds,
            IsFeaturedHero = film.IsFeaturedHero,
            IsPublished = film.IsPublished,
            DisplayOrder = film.DisplayOrder,
            ViewCount = film.ViewCount
        };
    }

    private FilmDetailDto MapToFilmDetailDto(Film film)
    {
        return new FilmDetailDto
        {
            Id = film.Id,
            Title = film.Title,
            Slug = film.Slug,
            CoupleNames = film.CoupleNames,
            ShootLocation = film.ShootLocation,
            CountryName = film.CountryName,
            Latitude = film.Latitude,
            Longitude = film.Longitude,
            ShootDate = film.ShootDate,
            Story = film.Story,
            HeroVideoUrl = film.HeroVideoUrl,
            TrailerUrl = film.TrailerUrl,
            VideoType = film.VideoType,
            EmbedId = film.EmbedId,
            PosterImagePath = film.PosterImagePath,
            ThumbnailPath = film.ThumbnailPath,
            BlurHash = film.BlurHash,
            DurationSeconds = film.DurationSeconds,
            IsFeaturedHero = film.IsFeaturedHero,
            ViewCount = film.ViewCount,
            CreatedAt = film.CreatedAt,
            GalleryImagePaths = film.GalleryImages.OrderBy(g => g.DisplayOrder).Select(g => g.ImagePath).ToList()
        };
    }

    private static string GenerateSlug(string title)
    {
        var slug = Regex.Replace(title.ToLower(), @"[^a-z0-9]+", "-");
        return Regex.Replace(slug, @"^-+|-+$", "");
    }
}
