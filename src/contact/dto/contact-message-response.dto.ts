import { IsOptional } from 'class-validator';

export class ContactMessageDto {
  id!: number;
  name!: string;
  email!: string;
  phone?: string | null;
  subject?: string | null;
  message!: string;
  eventType?: string | null;
  eventDate?: Date | null;
  isRead!: boolean;
  createdAt!: Date;
}

export class MarkContactMessageReadRequestDto {
  // @IsOptional() is a no-op validator kept only so `whitelist: true` doesn't strip this field.
  @IsOptional() isRead!: boolean;
}
