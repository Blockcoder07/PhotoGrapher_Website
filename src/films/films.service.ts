import { Injectable } from '@nestjs/common';
import { Film, Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service.js';
import { PaginatedResponse } from '../common/dto/api-response.dto.js';
import { CreateFilmRequestDto } from './dto/create-film-request.dto.js';
import { UpdateFilmRequestDto } from './dto/update-film-request.dto.js';
import { FilmDetailDto, FilmDto, FilmGalleryImageDto, FilmMapDto } from './dto/film-response.dto.js';

@Injectable()
export class FilmsService {
  constructor(private readonly prisma: PrismaService) {}

  getFilms(pageNumber = 1, pageSize = 20): Promise<PaginatedResponse<FilmDto>> {
    return this.paginateFilms({ isDeleted: false, isPublished: true }, pageNumber, pageSize);
  }

  getAllFilmsForAdmin(pageNumber = 1, pageSize = 20): Promise<PaginatedResponse<FilmDto>> {
    return this.paginateFilms({ isDeleted: false }, pageNumber, pageSize);
  }

  private async paginateFilms(
    where: Prisma.FilmWhereInput,
    pageNumber: number,
    pageSize: number,
  ): Promise<PaginatedResponse<FilmDto>> {
    const [totalCount, films] = await Promise.all([
      this.prisma.film.count({ where }),
      this.prisma.film.findMany({
        where,
        orderBy: { displayOrder: 'asc' },
        skip: (pageNumber - 1) * pageSize,
        take: pageSize,
      }),
    ]);

    return new PaginatedResponse({
      items: films.map(mapToFilmDto),
      pageNumber,
      pageSize,
      totalCount,
    });
  }

  async getFilmBySlug(slug: string): Promise<FilmDetailDto | null> {
    const film = await this.prisma.film.findFirst({
      where: { slug, isPublished: true, isDeleted: false },
      include: { galleryImages: { orderBy: { displayOrder: 'asc' } } },
    });
    if (!film) return null;

    await this.prisma.film.update({ where: { id: film.id }, data: { viewCount: { increment: 1 } } });
    film.viewCount += 1; // mirrors FilmService.cs incrementing the tracked entity before mapping

    return mapToFilmDetailDto(film);
  }

  async getHeroFilms(): Promise<FilmDto[]> {
    const films = await this.prisma.film.findMany({
      where: { isFeaturedHero: true, isPublished: true, isDeleted: false },
      orderBy: { displayOrder: 'asc' },
      take: 3,
    });
    return films.map(mapToFilmDto);
  }

  async getFilmsForMap(): Promise<FilmMapDto[]> {
    const films = await this.prisma.film.findMany({
      where: { isPublished: true, isDeleted: false },
    });
    return films.map((f) => ({
      id: f.id,
      slug: f.slug,
      coupleNames: f.coupleNames,
      shootLocation: f.shootLocation,
      countryName: f.countryName,
      latitude: Number(f.latitude),
      longitude: Number(f.longitude),
      posterImagePath: f.posterImagePath,
      paletteJson: f.paletteJson,
    }));
  }

  async createFilm(request: CreateFilmRequestDto): Promise<FilmDto> {
    if (request.latitude < -90 || request.latitude > 90 || request.longitude < -180 || request.longitude > 180) {
      throw new Error('Invalid coordinates');
    }
    if (request.isFeaturedHero && !request.heroVideoUrl) {
      throw new Error('Hero films must have a hero video URL');
    }

    const slug = generateSlug(request.title);
    const existing = await this.prisma.film.findFirst({ where: { slug, isDeleted: false } });
    if (existing) {
      throw new Error('A film with this title already exists');
    }

    const film = await this.prisma.film.create({
      data: {
        title: request.title,
        slug,
        coupleNames: request.coupleNames,
        shootLocation: request.shootLocation,
        countryName: request.countryName,
        latitude: request.latitude,
        longitude: request.longitude,
        shootDate: new Date(request.shootDate),
        story: request.story,
        quote: request.quote,
        heroVideoUrl: request.heroVideoUrl,
        trailerUrl: request.trailerUrl,
        videoType: request.videoType,
        embedId: request.embedId,
        posterImagePath: request.posterImagePath,
        webpPath: request.webpPath,
        thumbnailPath: request.thumbnailPath,
        blurHash: request.blurHash,
        lqipDataUri: request.lqipDataUri,
        paletteJson: request.paletteJson,
        durationSeconds: request.durationSeconds,
        isFeaturedHero: request.isFeaturedHero,
        isPublished: request.isPublished,
        displayOrder: request.displayOrder,
        viewCount: 0,
        isDeleted: false,
        createdAt: new Date(),
        updatedAt: new Date(),
      },
    });

    return mapToFilmDto(film);
  }

  async updateFilm(id: number, request: UpdateFilmRequestDto): Promise<FilmDto | null> {
    const film = await this.prisma.film.findFirst({ where: { id, isDeleted: false } });
    if (!film) return null;

    if (request.isFeaturedHero && !request.heroVideoUrl) {
      throw new Error('Hero films must have a hero video URL');
    }

    // Every field below is unconditionally overwritten (never conditionally skipped), matching
    // FilmService.cs's UpdateFilmAsync doing plain property assignment for each — including
    // nullable fields, which is why optional inputs are coerced to `?? null` rather than left
    // `undefined` (Prisma treats `undefined` as "don't touch this column", unlike C#'s assignment).
    const updated = await this.prisma.film.update({
      where: { id },
      data: {
        title: request.title,
        coupleNames: request.coupleNames,
        shootLocation: request.shootLocation,
        countryName: request.countryName,
        latitude: request.latitude,
        longitude: request.longitude,
        shootDate: new Date(request.shootDate),
        story: request.story,
        heroVideoUrl: request.heroVideoUrl,
        trailerUrl: request.trailerUrl,
        videoType: request.videoType,
        embedId: request.embedId ?? null,
        posterImagePath: request.posterImagePath ?? null,
        webpPath: request.webpPath ?? null,
        thumbnailPath: request.thumbnailPath ?? null,
        blurHash: request.blurHash ?? null,
        durationSeconds: request.durationSeconds ?? null,
        isFeaturedHero: request.isFeaturedHero,
        isPublished: request.isPublished,
        displayOrder: request.displayOrder,
        updatedAt: new Date(),
      },
    });

    return mapToFilmDto(updated);
  }

  async deleteFilm(id: number): Promise<boolean> {
    const film = await this.prisma.film.findFirst({ where: { id, isDeleted: false } });
    if (!film) return false;

    await this.prisma.film.update({ where: { id }, data: { isDeleted: true, updatedAt: new Date() } });
    return true;
  }

  async getGalleryImages(filmId: number): Promise<FilmGalleryImageDto[]> {
    const images = await this.prisma.filmGalleryImage.findMany({
      where: { filmId, film: { isDeleted: false } },
      orderBy: { displayOrder: 'asc' },
    });
    return images.map((g) => ({ id: g.id, imagePath: g.imagePath, displayOrder: g.displayOrder }));
  }

  async addGalleryImage(filmId: number, imagePath: string): Promise<FilmGalleryImageDto | null> {
    const filmExists = await this.prisma.film.findFirst({ where: { id: filmId, isDeleted: false } });
    if (!filmExists) return null;

    const maxOrder = await this.prisma.filmGalleryImage.aggregate({
      where: { filmId },
      _max: { displayOrder: true },
    });

    const image = await this.prisma.filmGalleryImage.create({
      data: {
        filmId,
        imagePath,
        displayOrder: (maxOrder._max.displayOrder ?? -1) + 1,
        isDeleted: false,
        createdAt: new Date(),
        updatedAt: new Date(),
      },
    });

    return { id: image.id, imagePath: image.imagePath, displayOrder: image.displayOrder };
  }

  async deleteGalleryImage(filmId: number, imageId: number): Promise<string | null> {
    const image = await this.prisma.filmGalleryImage.findFirst({
      where: { id: imageId, filmId, film: { isDeleted: false } },
    });
    if (!image) return null;

    await this.prisma.filmGalleryImage.delete({ where: { id: image.id } });
    return image.imagePath;
  }

  async reorderGalleryImages(filmId: number, orderedImageIds: number[]): Promise<void> {
    const images = await this.prisma.filmGalleryImage.findMany({
      where: { filmId, film: { isDeleted: false } },
    });
    const byId = new Map(images.map((img) => [img.id, img]));

    await this.prisma.$transaction(
      orderedImageIds
        .map((imageId, index) => ({ imageId, index }))
        .filter(({ imageId }) => byId.has(imageId))
        .map(({ imageId, index }) =>
          this.prisma.filmGalleryImage.update({ where: { id: imageId }, data: { displayOrder: index } }),
        ),
    );
  }
}

function generateSlug(title: string): string {
  const slug = title.toLowerCase().replace(/[^a-z0-9]+/g, '-');
  return slug.replace(/^-+|-+$/g, '');
}

function mapToFilmDto(film: Film): FilmDto {
  return {
    id: film.id,
    title: film.title,
    slug: film.slug,
    coupleNames: film.coupleNames,
    shootLocation: film.shootLocation,
    countryName: film.countryName,
    latitude: Number(film.latitude),
    longitude: Number(film.longitude),
    shootDate: film.shootDate,
    story: film.story,
    quote: film.quote,
    heroVideoUrl: film.heroVideoUrl,
    trailerUrl: film.trailerUrl,
    videoType: film.videoType,
    embedId: film.embedId,
    posterImagePath: film.posterImagePath,
    webpPath: film.webpPath,
    thumbnailPath: film.thumbnailPath,
    blurHash: film.blurHash,
    lqipDataUri: film.lqipDataUri,
    paletteJson: film.paletteJson,
    durationSeconds: film.durationSeconds,
    isFeaturedHero: film.isFeaturedHero,
    isPublished: film.isPublished,
    displayOrder: film.displayOrder,
    viewCount: film.viewCount,
  };
}

function mapToFilmDetailDto(film: Film & { galleryImages: { imagePath: string; displayOrder: number }[] }): FilmDetailDto {
  return {
    id: film.id,
    title: film.title,
    slug: film.slug,
    coupleNames: film.coupleNames,
    shootLocation: film.shootLocation,
    countryName: film.countryName,
    latitude: Number(film.latitude),
    longitude: Number(film.longitude),
    shootDate: film.shootDate,
    story: film.story,
    heroVideoUrl: film.heroVideoUrl,
    trailerUrl: film.trailerUrl,
    videoType: film.videoType,
    embedId: film.embedId,
    posterImagePath: film.posterImagePath,
    thumbnailPath: film.thumbnailPath,
    blurHash: film.blurHash,
    durationSeconds: film.durationSeconds,
    isFeaturedHero: film.isFeaturedHero,
    viewCount: film.viewCount,
    createdAt: film.createdAt,
    galleryImagePaths: film.galleryImages.map((g) => g.imagePath),
  };
}
