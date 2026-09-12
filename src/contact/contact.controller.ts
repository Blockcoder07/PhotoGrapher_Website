import { Body, Controller, Post } from '@nestjs/common';
import { ContactService } from './contact.service.js';
import { SubmitContactMessageRequestDto } from './dto/submit-contact-message-request.dto.js';
import { ApiResponse } from '../common/dto/api-response.dto.js';

@Controller('api/contact')
export class ContactController {
  constructor(private readonly contactService: ContactService) {}

  @Post()
  async submit(@Body() dto: SubmitContactMessageRequestDto) {
    await this.contactService.submit(dto);
    return ApiResponse.ok('Message received');
  }
}
