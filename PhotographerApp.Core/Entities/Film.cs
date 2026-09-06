using PhotographerApp.Core.Enums;

namespace PhotographerApp.Core.Entities;

public class Film : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CoupleNames { get; set; } = string.Empty;
    public string ShootLocation { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public DateTime ShootDate { get; set; }
    public string Story { get; set; } = string.Empty;
    public string? Quote { get; set; } // Film quote for hero display

    public string HeroVideoUrl { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public VideoType VideoType { get; set; }
    public string? EmbedId { get; set; }

    public string? PosterImagePath { get; set; }
    public string? WebpPath { get; set; } // WebP variant of poster
    public string? ThumbnailPath { get; set; }
    public string? BlurHash { get; set; }
    public string? LqipDataUri { get; set; } // Low Quality Image Placeholder
    public string? PaletteJson { get; set; } // ["#hex","#hex","#hex"] for placeholder gradients
    public int? DurationSeconds { get; set; }

    public bool IsFeaturedHero { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public int ViewCount { get; set; }

    public ICollection<FilmGalleryImage> GalleryImages { get; set; } = new List<FilmGalleryImage>();
}
