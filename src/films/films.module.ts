import { Module } from '@nestjs/common';
import { FilmsController } from './films.controller.js';
import { AdminFilmsController } from './admin-films.controller.js';
import { FilmsService } from './films.service.js';
import { FilesModule } from '../files/files.module.js';
import { AuthModule } from '../auth/auth.module.js';

@Module({
  imports: [FilesModule, AuthModule],
  controllers: [FilmsController, AdminFilmsController],
  providers: [FilmsService],
})
export class FilmsModule {}
