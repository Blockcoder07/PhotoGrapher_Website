using Microsoft.EntityFrameworkCore;
using PhotographerApp.Core.Entities;
using PhotographerApp.Core.Enums;
using PhotographerApp.Infrastructure.Data;

namespace PhotographerApp.Infrastructure.Data.Seeders;

public class FilmSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Films.AnyAsync())
            return;

        // Create films
        var films = new List<Film>
        {
            new Film
            {
                Title = "Riya & Arjun",
                Slug = "riya-arjun",
                CoupleNames = "Riya & Arjun",
                ShootLocation = "Mumbai, India",
                CountryName = "India",
                Latitude = 19.0760m,
                Longitude = 72.8777m,
                ShootDate = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                Story = "A beautiful union of two families celebrating love under the mandap. Rose petals fell as they exchanged vows, capturing the essence of an Indian wedding's grace and joy.",
                HeroVideoUrl = "/uploads/videos/hero/riya-arjun-hero.mp4",
                TrailerUrl = "/uploads/videos/trailers/riya-arjun.mp4",
                VideoType = VideoType.Uploaded,
                PosterImagePath = "/uploads/photos/full/mandap-rose-petals.jpg",
                ThumbnailPath = "/uploads/photos/thumb/mandap-rose-petals.jpg",
                DurationSeconds = 420,
                IsFeaturedHero = true,
                IsPublished = true,
                DisplayOrder = 1,
                ViewCount = 245
            },
            new Film
            {
                Title = "Priya & Vikram",
                Slug = "priya-vikram",
                CoupleNames = "Priya & Vikram",
                ShootLocation = "Delhi, India",
                CountryName = "India",
                Latitude = 28.6139m,
                Longitude = 77.2090m,
                ShootDate = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc),
                Story = "An elegant red lehenga and royal golden sherwani set the stage for a celebration of tradition and modernity. Every moment captured tells a story of love, family, and belonging.",
                HeroVideoUrl = "/uploads/videos/hero/priya-vikram-hero.mp4",
                TrailerUrl = "/uploads/videos/trailers/priya-vikram.mp4",
                VideoType = VideoType.Uploaded,
                PosterImagePath = "/uploads/photos/full/bride-entrance-red.jpg",
                ThumbnailPath = "/uploads/photos/thumb/bride-entrance-red.jpg",
                DurationSeconds = 385,
                IsFeaturedHero = true,
                IsPublished = true,
                DisplayOrder = 2,
                ViewCount = 189
            },
            new Film
            {
                Title = "Neha & Rohan",
                Slug = "neha-rohan",
                CoupleNames = "Neha & Rohan",
                ShootLocation = "Bangalore, India",
                CountryName = "India",
                Latitude = 12.9716m,
                Longitude = 77.5946m,
                ShootDate = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Story = "Sunshine yellow and joy painted the morning of their haldi. Two souls in perfect harmony, decorated with turmeric and smiles, ready to begin forever together.",
                HeroVideoUrl = "/uploads/videos/hero/neha-rohan-hero.mp4",
                TrailerUrl = "/uploads/videos/trailers/neha-rohan.mp4",
                VideoType = VideoType.Uploaded,
                PosterImagePath = "/uploads/photos/full/haldi-couple-yellow.jpg",
                ThumbnailPath = "/uploads/photos/thumb/haldi-couple-yellow.jpg",
                DurationSeconds = 365,
                IsFeaturedHero = true,
                IsPublished = true,
                DisplayOrder = 3,
                ViewCount = 312
            },
            new Film
            {
                Title = "Anjali & Harsh",
                Slug = "anjali-harsh",
                CoupleNames = "Anjali & Harsh",
                ShootLocation = "Pune, India",
                CountryName = "India",
                Latitude = 18.5204m,
                Longitude = 73.8567m,
                ShootDate = new DateTime(2023, 12, 28, 0, 0, 0, DateTimeKind.Utc),
                Story = "Under the stars, after the last ritual, they held each other as if the rest of the world had faded away. A moment of pure intimacy, a night they'll never forget.",
                HeroVideoUrl = "/uploads/videos/hero/anjali-harsh-hero.mp4",
                TrailerUrl = "/uploads/videos/trailers/anjali-harsh.mp4",
                VideoType = VideoType.Uploaded,
                PosterImagePath = "/uploads/photos/full/night-embrace.jpg",
                ThumbnailPath = "/uploads/photos/thumb/night-embrace.jpg",
                DurationSeconds = 445,
                IsFeaturedHero = false,
                IsPublished = true,
                DisplayOrder = 4,
                ViewCount = 156
            },
            new Film
            {
                Title = "Sakshi & Mahesh",
                Slug = "sakshi-mahesh",
                CoupleNames = "Sakshi & Mahesh",
                ShootLocation = "Aurangabad, India",
                CountryName = "India",
                Latitude = 19.8750m,
                Longitude = 75.3458m,
                ShootDate = new DateTime(2023, 11, 5, 0, 0, 0, DateTimeKind.Utc),
                Story = "A Maharashtrian tradition honored in every frame. The mangalsutra ceremony, sacred and solemn, marked the moment two became one forever in this beautiful binding ritual.",
                HeroVideoUrl = "/uploads/videos/hero/sakshi-mahesh-hero.mp4",
                TrailerUrl = "/uploads/videos/trailers/sakshi-mahesh.mp4",
                VideoType = VideoType.Uploaded,
                PosterImagePath = "/uploads/photos/full/mangalsutra-moment.jpg",
                ThumbnailPath = "/uploads/photos/thumb/mangalsutra-moment.jpg",
                DurationSeconds = 392,
                IsFeaturedHero = false,
                IsPublished = true,
                DisplayOrder = 5,
                ViewCount = 203
            },
            new Film
            {
                Title = "Karan & Shreya",
                Slug = "karan-shreya",
                CoupleNames = "Karan & Shreya",
                ShootLocation = "Jaipur, India",
                CountryName = "India",
                Latitude = 26.9124m,
                Longitude = 75.7873m,
                ShootDate = new DateTime(2023, 10, 14, 0, 0, 0, DateTimeKind.Utc),
                Story = "Standing at the mandap in regal cream and red, waiting for the pheras to begin. The anticipation in his eyes, the grace in her presence—a moment before forever changes.",
                HeroVideoUrl = "/uploads/videos/hero/karan-shreya-hero.mp4",
                TrailerUrl = "/uploads/videos/trailers/karan-shreya.mp4",
                VideoType = VideoType.Uploaded,
                PosterImagePath = "/uploads/photos/full/groom-portrait-mandap.jpg",
                ThumbnailPath = "/uploads/photos/thumb/groom-portrait-mandap.jpg",
                DurationSeconds = 418,
                IsFeaturedHero = false,
                IsPublished = true,
                DisplayOrder = 6,
                ViewCount = 178
            },
            new Film
            {
                Title = "Divya & Nikhil",
                Slug = "divya-nikhil",
                CoupleNames = "Divya & Nikhil",
                ShootLocation = "Hyderabad, India",
                CountryName = "India",
                Latitude = 17.3850m,
                Longitude = 78.4867m,
                ShootDate = new DateTime(2023, 9, 22, 0, 0, 0, DateTimeKind.Utc),
                Story = "In the quiet moments before the celebration, every detail tells a story. The tiara, the shoes, the jewels—each piece a reflection of dreams and anticipation.",
                HeroVideoUrl = "/uploads/videos/hero/divya-nikhil-hero.mp4",
                TrailerUrl = "/uploads/videos/trailers/divya-nikhil.mp4",
                VideoType = VideoType.Uploaded,
                PosterImagePath = "/uploads/photos/full/detail-shoes-tiara.jpg",
                ThumbnailPath = "/uploads/photos/thumb/detail-shoes-tiara.jpg",
                DurationSeconds = 356,
                IsFeaturedHero = false,
                IsPublished = true,
                DisplayOrder = 7,
                ViewCount = 267
            }
        };

        context.Films.AddRange(films);
        await context.SaveChangesAsync();
    }
}
