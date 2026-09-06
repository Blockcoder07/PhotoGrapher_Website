using PhotographerApp.Core.Entities;
using PhotographerApp.Core.Enums;

namespace PhotographerApp.Core.DTOs;

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
}

// Request DTOs
public class CreateFilmRequest
{
    public string Title { get; set; } = string.Empty;
    public string CoupleNames { get; set; } = string.Empty;
    public string ShootLocation { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime ShootDate { get; set; }
    public string Story { get; set; } = string.Empty;
    public string? Quote { get; set; }
    public string HeroVideoUrl { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public VideoType VideoType { get; set; }
    public string? EmbedId { get; set; }
    public string? PosterImagePath { get; set; }
    public string? WebpPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? BlurHash { get; set; }
    public string? LqipDataUri { get; set; }
    public string? PaletteJson { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsFeaturedHero { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateFilmRequest
{
    public string Title { get; set; } = string.Empty;
    public string CoupleNames { get; set; } = string.Empty;
    public string ShootLocation { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime ShootDate { get; set; }
    public string Story { get; set; } = string.Empty;
    public string HeroVideoUrl { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public VideoType VideoType { get; set; }
    public string? EmbedId { get; set; }
    public string? PosterImagePath { get; set; }
    public string? WebpPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? BlurHash { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsFeaturedHero { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
}

// Response DTOs
public class FilmDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CoupleNames { get; set; } = string.Empty;
    public string ShootLocation { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime ShootDate { get; set; }
    public string Story { get; set; } = string.Empty;
    public string? Quote { get; set; }
    public string HeroVideoUrl { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public VideoType VideoType { get; set; }
    public string? EmbedId { get; set; }
    public string? PosterImagePath { get; set; }
    public string? WebpPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? BlurHash { get; set; }
    public string? LqipDataUri { get; set; }
    public string? PaletteJson { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsFeaturedHero { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public int ViewCount { get; set; }
}

public class FilmDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CoupleNames { get; set; } = string.Empty;
    public string ShootLocation { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime ShootDate { get; set; }
    public string Story { get; set; } = string.Empty;
    public string HeroVideoUrl { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public VideoType VideoType { get; set; }
    public string? EmbedId { get; set; }
    public string? PosterImagePath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? BlurHash { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsFeaturedHero { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> GalleryImagePaths { get; set; } = new();
}

public class FilmGalleryImageDto
{
    public int Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class ReorderGalleryImagesRequest
{
    public List<int> OrderedImageIds { get; set; } = new();
}

public class FilmMapDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string CoupleNames { get; set; } = string.Empty;
    public string ShootLocation { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? PosterImagePath { get; set; }
    public string? PaletteJson { get; set; }
}

