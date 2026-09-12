import { Injectable } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service.js';
import { PaginatedResponse } from '../common/dto/api-response.dto.js';
import { SubmitContactMessageRequestDto } from './dto/submit-contact-message-request.dto.js';
import { ContactMessageDto } from './dto/contact-message-response.dto.js';

@Injectable()
export class ContactService {
  constructor(private readonly prisma: PrismaService) {}

  async submit(request: SubmitContactMessageRequestDto): Promise<void> {
    await this.prisma.contactMessage.create({
      data: {
        name: request.name,
        email: request.email,
        subject: request.subject,
        message: request.message,
        isRead: false,
        isDeleted: false,
        createdAt: new Date(),
        updatedAt: new Date(),
      },
    });
  }

  async getMessages(pageNumber = 1, pageSize = 20): Promise<PaginatedResponse<ContactMessageDto>> {
    const where = { isDeleted: false };
    const [totalCount, messages] = await Promise.all([
      this.prisma.contactMessage.count({ where }),
      this.prisma.contactMessage.findMany({
        where,
        orderBy: { createdAt: 'desc' },
        skip: (pageNumber - 1) * pageSize,
        take: pageSize,
      }),
    ]);

    return new PaginatedResponse({
      items: messages.map((m) => ({
        id: m.id,
        name: m.name,
        email: m.email,
        phone: m.phone,
        subject: m.subject,
        message: m.message,
        eventType: m.eventType,
        eventDate: m.eventDate,
        isRead: m.isRead,
        createdAt: m.createdAt,
      })),
      pageNumber,
      pageSize,
      totalCount,
    });
  }

  async setRead(id: number, isRead: boolean): Promise<boolean> {
    const message = await this.prisma.contactMessage.findFirst({ where: { id, isDeleted: false } });
    if (!message) return false;

    await this.prisma.contactMessage.update({ where: { id }, data: { isRead, updatedAt: new Date() } });
    return true;
  }

  async delete(id: number): Promise<boolean> {
    const message = await this.prisma.contactMessage.findFirst({ where: { id, isDeleted: false } });
    if (!message) return false;

    await this.prisma.contactMessage.update({ where: { id }, data: { isDeleted: true, updatedAt: new Date() } });
    return true;
  }
}
