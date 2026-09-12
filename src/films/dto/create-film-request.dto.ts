import { IsOptional } from 'class-validator';
import { VideoType } from '../video-type.enum.js';

/** No FluentValidation existed for Films DTOs in the .NET app either — domain checks
 * (coordinate range, hero-video-required-if-featured, slug uniqueness) live in FilmsService,
 * mirroring FilmService.cs. `@IsOptional()` here is a no-op validator, not a real constraint —
 * it only exists so Nest's global `whitelist: true` ValidationPipe recognizes these as known
 * properties instead of stripping them (whitelist drops any property with zero decorators). */
export class CreateFilmRequestDto {
  @IsOptional() title!: string;
  @IsOptional() coupleNames!: string;
  @IsOptional() shootLocation!: string;
  @IsOptional() countryName!: string;
  @IsOptional() latitude!: number;
  @IsOptional() longitude!: number;
  @IsOptional() shootDate!: string;
  @IsOptional() story!: string;
  @IsOptional() quote?: string;
  @IsOptional() heroVideoUrl!: string;
  @IsOptional() trailerUrl!: string;
  @IsOptional() videoType!: VideoType;
  @IsOptional() embedId?: string;
  @IsOptional() posterImagePath?: string;
  @IsOptional() webpPath?: string;
  @IsOptional() thumbnailPath?: string;
  @IsOptional() blurHash?: string;
  @IsOptional() lqipDataUri?: string;
  @IsOptional() paletteJson?: string;
  @IsOptional() durationSeconds?: number;
  @IsOptional() isFeaturedHero!: boolean;
  @IsOptional() isPublished!: boolean;
  @IsOptional() displayOrder!: number;
}
