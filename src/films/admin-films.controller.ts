import {
  BadRequestException,
  Body,
  Controller,
  DefaultValuePipe,
  Delete,
  Get,
  HttpException,
  InternalServerErrorException,
  NotFoundException,
  Param,
  ParseIntPipe,
  Post,
  Put,
  Query,
  UploadedFile,
  UseGuards,
  UseInterceptors,
} from '@nestjs/common';
import { FileInterceptor } from '@nestjs/platform-express';
import { memoryStorage } from 'multer';
import { FilmsService } from './films.service.js';
import { FileStorageService } from '../files/file-storage.service.js';
import { ApiResponse } from '../common/dto/api-response.dto.js';
import { CreateFilmRequestDto } from './dto/create-film-request.dto.js';
import { UpdateFilmRequestDto } from './dto/update-film-request.dto.js';
import { ReorderGalleryImagesRequestDto } from './dto/film-response.dto.js';
import { JwtAuthGuard } from '../auth/jwt-auth.guard.js';
import { RolesGuard } from '../auth/roles.guard.js';
import { Roles } from '../auth/roles.decorator.js';

@Controller('api/admin/films')
@UseGuards(JwtAuthGuard, RolesGuard)
@Roles('Admin')
export class AdminFilmsController {
  constructor(
    private readonly filmsService: FilmsService,
    private readonly fileStorageService: FileStorageService,
  ) {}

  @Get()
  async getAllFilms(
    @Query('page', new DefaultValuePipe(1), ParseIntPipe) page: number,
    @Query('pageSize', new DefaultValuePipe(20), ParseIntPipe) pageSize: number,
  ) {
    const result = await this.filmsService.getAllFilmsForAdmin(page, pageSize);
    return ApiResponse.ok('Films retrieved', result);
  }

  @Post()
  async createFilm(@Body() dto: CreateFilmRequestDto) {
    // FilmsService throws a plain Error for domain-validation failures (bad coordinates, missing
    // hero video, duplicate slug) — left uncaught here so it becomes a generic 500 via the global
    // exception filter, matching GlobalExceptionMiddleware.cs: InvalidOperationException isn't an
    // ArgumentException, so it falls through to that switch's default (500) case in the .NET app too.
    const film = await this.filmsService.createFilm(dto);
    return ApiResponse.ok('Film created', film);
  }

  @Put(':id')
  async updateFilm(@Param('id', ParseIntPipe) id: number, @Body() dto: UpdateFilmRequestDto) {
    const film = await this.filmsService.updateFilm(id, dto);
    if (!film) throw new NotFoundException('Film not found');
    return ApiResponse.ok('Film updated', film);
  }

  @Delete(':id')
  async deleteFilm(@Param('id', ParseIntPipe) id: number) {
    const success = await this.filmsService.deleteFilm(id);
    if (!success) throw new NotFoundException('Film not found');
    return ApiResponse.ok('Film deleted');
  }

  @Post('upload')
  @UseInterceptors(FileInterceptor('file', { storage: memoryStorage() }))
  async uploadPoster(@UploadedFile() file?: Express.Multer.File) {
    if (!file || file.size === 0) throw new BadRequestException('No file provided');

    const validation = this.fileStorageService.validateImageFile(file);
    if (!validation.valid) throw new BadRequestException(validation.error);

    try {
      const filePath = await this.fileStorageService.savePhoto(file);
      return ApiResponse.ok('File uploaded successfully', filePath);
    } catch (err) {
      throw new InternalServerErrorException(`Upload failed: ${err instanceof Error ? err.message : 'unknown error'}`);
    }
  }

  @Get(':id/gallery')
  async getGallery(@Param('id', ParseIntPipe) id: number) {
    const images = await this.filmsService.getGalleryImages(id);
    return ApiResponse.ok('Gallery retrieved', images);
  }

  @Post(':id/gallery')
  @UseInterceptors(FileInterceptor('file', { storage: memoryStorage() }))
  async addGalleryImage(@Param('id', ParseIntPipe) id: number, @UploadedFile() file?: Express.Multer.File) {
    if (!file || file.size === 0) throw new BadRequestException('No file provided');

    const validation = this.fileStorageService.validateImageFile(file);
    if (!validation.valid) throw new BadRequestException(validation.error);

    try {
      const imagePath = await this.fileStorageService.savePhoto(file);
      const image = await this.filmsService.addGalleryImage(id, imagePath);
      if (!image) throw new NotFoundException('Film not found');
      return ApiResponse.ok('Photo added', image);
    } catch (err) {
      if (err instanceof HttpException) throw err;
      throw new InternalServerErrorException(`Upload failed: ${err instanceof Error ? err.message : 'unknown error'}`);
    }
  }

  @Delete(':id/gallery/:imageId')
  async deleteGalleryImage(@Param('id', ParseIntPipe) id: number, @Param('imageId', ParseIntPipe) imageId: number) {
    const imagePath = await this.filmsService.deleteGalleryImage(id, imageId);
    if (imagePath === null) throw new NotFoundException('Photo not found');

    await this.fileStorageService.deleteFile(imagePath);
    return ApiResponse.ok('Photo deleted');
  }

  @Put(':id/gallery/reorder')
  async reorderGallery(@Param('id', ParseIntPipe) id: number, @Body() dto: ReorderGalleryImagesRequestDto) {
    await this.filmsService.reorderGalleryImages(id, dto.orderedImageIds);
    return ApiResponse.ok('Reordered');
  }
}
