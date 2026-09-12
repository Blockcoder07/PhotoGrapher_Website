import { IsOptional } from 'class-validator';
import { VideoType } from '../video-type.enum.js';

export class FilmDto {
  id!: number;
  title!: string;
  slug!: string;
  coupleNames!: string;
  shootLocation!: string;
  countryName!: string;
  latitude!: number;
  longitude!: number;
  shootDate!: Date;
  story!: string;
  quote?: string | null;
  heroVideoUrl!: string;
  trailerUrl!: string;
  videoType!: VideoType;
  embedId?: string | null;
  posterImagePath?: string | null;
  webpPath?: string | null;
  thumbnailPath?: string | null;
  blurHash?: string | null;
  lqipDataUri?: string | null;
  paletteJson?: string | null;
  durationSeconds?: number | null;
  isFeaturedHero!: boolean;
  isPublished!: boolean;
  displayOrder!: number;
  viewCount!: number;
}

export class FilmDetailDto {
  id!: number;
  title!: string;
  slug!: string;
  coupleNames!: string;
  shootLocation!: string;
  countryName!: string;
  latitude!: number;
  longitude!: number;
  shootDate!: Date;
  story!: string;
  heroVideoUrl!: string;
  trailerUrl!: string;
  videoType!: VideoType;
  embedId?: string | null;
  posterImagePath?: string | null;
  thumbnailPath?: string | null;
  blurHash?: string | null;
  durationSeconds?: number | null;
  isFeaturedHero!: boolean;
  viewCount!: number;
  createdAt!: Date;
  galleryImagePaths!: string[];
}

export class FilmGalleryImageDto {
  id!: number;
  imagePath!: string;
  displayOrder!: number;
}

export class FilmMapDto {
  id!: number;
  slug!: string;
  coupleNames!: string;
  shootLocation!: string;
  countryName!: string;
  latitude!: number;
  longitude!: number;
  posterImagePath?: string | null;
  paletteJson?: string | null;
}

export class ReorderGalleryImagesRequestDto {
  // @IsOptional() is a no-op validator kept only so `whitelist: true` doesn't strip this field.
  @IsOptional() orderedImageIds!: number[];
}
