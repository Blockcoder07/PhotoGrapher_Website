import { IsOptional } from 'class-validator';
import { VideoType } from '../video-type.enum.js';

/** Mirrors UpdateFilmRequest in FilmDtos.cs — deliberately missing `quote`, `lqipDataUri` and
 * `paletteJson` (present on CreateFilmRequestDto): the .NET update endpoint never touches those
 * three columns, so an update call leaves their existing DB values untouched. Not an omission.
 * `@IsOptional()` is a no-op validator kept only so `whitelist: true` doesn't strip these fields
 * (see CreateFilmRequestDto for the same note). */
export class UpdateFilmRequestDto {
  @IsOptional() title!: string;
  @IsOptional() coupleNames!: string;
  @IsOptional() shootLocation!: string;
  @IsOptional() countryName!: string;
  @IsOptional() latitude!: number;
  @IsOptional() longitude!: number;
  @IsOptional() shootDate!: string;
  @IsOptional() story!: string;
  @IsOptional() heroVideoUrl!: string;
  @IsOptional() trailerUrl!: string;
  @IsOptional() videoType!: VideoType;
  @IsOptional() embedId?: string;
  @IsOptional() posterImagePath?: string;
  @IsOptional() webpPath?: string;
  @IsOptional() thumbnailPath?: string;
  @IsOptional() blurHash?: string;
  @IsOptional() durationSeconds?: number;
  @IsOptional() isFeaturedHero!: boolean;
  @IsOptional() isPublished!: boolean;
  @IsOptional() displayOrder!: number;
}
