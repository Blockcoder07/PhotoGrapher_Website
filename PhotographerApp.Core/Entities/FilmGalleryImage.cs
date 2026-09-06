namespace PhotographerApp.Core.Entities;

/// <summary>A single "behind the scenes" photo attached to a Film.</summary>
public class FilmGalleryImage : BaseEntity
{
    public int FilmId { get; set; }
    public Film Film { get; set; } = null!;
    public string ImagePath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
