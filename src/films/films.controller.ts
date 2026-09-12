import { Controller, DefaultValuePipe, Get, NotFoundException, Param, ParseIntPipe, Query } from '@nestjs/common';
import { FilmsService } from './films.service.js';
import { ApiResponse } from '../common/dto/api-response.dto.js';

@Controller('api/films')
export class FilmsController {
  constructor(private readonly filmsService: FilmsService) {}

  @Get()
  async getFilms(
    @Query('page', new DefaultValuePipe(1), ParseIntPipe) page: number,
    @Query('pageSize', new DefaultValuePipe(20), ParseIntPipe) pageSize: number,
  ) {
    const result = await this.filmsService.getFilms(page, pageSize);
    return ApiResponse.ok('Films retrieved', result);
  }

  @Get('hero')
  async getHeroFilms() {
    const films = await this.filmsService.getHeroFilms();
    return ApiResponse.ok('Hero films retrieved', films);
  }

  @Get('map')
  async getFilmsForMap() {
    const films = await this.filmsService.getFilmsForMap();
    return ApiResponse.ok('Map data retrieved', films);
  }

  @Get(':slug')
  async getFilmBySlug(@Param('slug') slug: string) {
    const film = await this.filmsService.getFilmBySlug(slug);
    if (!film) throw new NotFoundException('Film not found');
    return ApiResponse.ok('Film retrieved', film);
  }
}
