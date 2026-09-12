import {
  Body,
  Controller,
  DefaultValuePipe,
  Delete,
  Get,
  NotFoundException,
  Param,
  ParseIntPipe,
  Patch,
  Query,
  UseGuards,
} from '@nestjs/common';
import { ContactService } from './contact.service.js';
import { MarkContactMessageReadRequestDto } from './dto/contact-message-response.dto.js';
import { ApiResponse } from '../common/dto/api-response.dto.js';
import { JwtAuthGuard } from '../auth/jwt-auth.guard.js';
import { RolesGuard } from '../auth/roles.guard.js';
import { Roles } from '../auth/roles.decorator.js';

@Controller('api/admin/contactmessages')
@UseGuards(JwtAuthGuard, RolesGuard)
@Roles('Admin')
export class AdminContactController {
  constructor(private readonly contactService: ContactService) {}

  @Get()
  async getAll(
    @Query('page', new DefaultValuePipe(1), ParseIntPipe) page: number,
    @Query('pageSize', new DefaultValuePipe(20), ParseIntPipe) pageSize: number,
  ) {
    const result = await this.contactService.getMessages(page, pageSize);
    return ApiResponse.ok('Messages retrieved', result);
  }

  @Patch(':id/read')
  async setRead(@Param('id', ParseIntPipe) id: number, @Body() dto: MarkContactMessageReadRequestDto) {
    const success = await this.contactService.setRead(id, dto.isRead);
    if (!success) throw new NotFoundException('Message not found');
    return ApiResponse.ok('Updated');
  }

  @Delete(':id')
  async delete(@Param('id', ParseIntPipe) id: number) {
    const success = await this.contactService.delete(id);
    if (!success) throw new NotFoundException('Message not found');
    return ApiResponse.ok('Deleted');
  }
}
