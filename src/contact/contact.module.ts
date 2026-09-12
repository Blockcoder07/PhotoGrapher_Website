import { Module } from '@nestjs/common';
import { ContactController } from './contact.controller.js';
import { AdminContactController } from './admin-contact.controller.js';
import { ContactService } from './contact.service.js';
import { AuthModule } from '../auth/auth.module.js';

@Module({
  imports: [AuthModule],
  controllers: [ContactController, AdminContactController],
  providers: [ContactService],
})
export class ContactModule {}
