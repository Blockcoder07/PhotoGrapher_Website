using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotographerApp.Core.Entities;

namespace PhotographerApp.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    // Film-led content
    public DbSet<Film> Films => Set<Film>();
    public DbSet<FilmGalleryImage> FilmGalleryImages => Set<FilmGalleryImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(300);
            entity.Property(e => e.Message).HasColumnType("text").IsRequired();
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Film entity
        modelBuilder.Entity<Film>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(220).IsRequired();
            entity.Property(e => e.CoupleNames).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ShootLocation).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CountryName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasPrecision(9, 6);
            entity.Property(e => e.Story).HasColumnType("text");
            entity.Property(e => e.Quote).HasMaxLength(500);
            entity.Property(e => e.HeroVideoUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.TrailerUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.EmbedId).HasMaxLength(100);
            entity.Property(e => e.PosterImagePath).HasMaxLength(500);
            entity.Property(e => e.WebpPath).HasMaxLength(500);
            entity.Property(e => e.ThumbnailPath).HasMaxLength(500);
            entity.Property(e => e.BlurHash).HasMaxLength(60);
            entity.Property(e => e.LqipDataUri).HasColumnType("text");
            entity.Property(e => e.PaletteJson).HasMaxLength(100); // JSON array of 3 hex colors
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => new { e.IsPublished, e.DisplayOrder });
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Film gallery ("behind the scenes" photos)
        modelBuilder.Entity<FilmGalleryImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImagePath).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.Film)
                .WithMany(f => f.GalleryImages)
                .HasForeignKey(e => e.FilmId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.FilmId, e.DisplayOrder });
            entity.HasQueryFilter(e => !e.Film.IsDeleted);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
