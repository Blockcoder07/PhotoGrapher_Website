import * as path from 'node:path';
import { ValidationPipe } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { NestFactory } from '@nestjs/core';
import { NestExpressApplication } from '@nestjs/platform-express';
import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';
import { AppModule } from './app.module.js';
import { AllExceptionsFilter } from './common/filters/http-exception.filter.js';

// Mirrors Program.cs's "AllowReactDev" CORS policy exactly, including the Vercel preview-deploy
// prefix match (every Vercel deployment gets its own subdomain like photo-grapher-fronted-<hash>.vercel.app).
const ALLOWED_ORIGINS = new Set([
  'http://localhost:5173',
  'http://localhost:3000',
  'https://localhost:5173',
  'https://localhost:3000',
  'https://both-dense-elevation.ngrok-free.dev',
  'http://both-dense-elevation.ngrok-free.dev',
]);

function isOriginAllowed(origin: string): boolean {
  if (ALLOWED_ORIGINS.has(origin)) return true;
  try {
    const url = new URL(origin);
    return (
      url.protocol === 'https:' &&
      url.hostname.toLowerCase().endsWith('.vercel.app') &&
      url.hostname.toLowerCase().startsWith('photo-grapher-fronted')
    );
  } catch {
    return false;
  }
}

async function bootstrap() {
  const app = await NestFactory.create<NestExpressApplication>(AppModule);
  const configService = app.get(ConfigService);

  app.enableCors({
    origin(origin, callback) {
      if (!origin || isOriginAllowed(origin)) {
        callback(null, true);
      } else {
        callback(null, false);
      }
    },
    credentials: true,
  });

  app.useGlobalPipes(new ValidationPipe({ whitelist: true, transform: true }));
  app.useGlobalFilters(new AllExceptionsFilter());

  // Mirrors Program.cs's app.UseStaticFiles over the whole wwwroot (both its "media" and
  // "uploads" subfolders get the same permissive headers) — wwwroot only ever had those two.
  const staticFileHeaders = (res: import('express').Response) => {
    res.set('Access-Control-Allow-Origin', '*');
    res.set('Access-Control-Allow-Methods', 'GET, OPTIONS');
    res.set('Access-Control-Allow-Headers', 'Content-Type, Authorization');
  };
  app.useStaticAssets(path.join(process.cwd(), 'uploads'), { prefix: '/uploads', setHeaders: staticFileHeaders });
  app.useStaticAssets(path.join(process.cwd(), 'media'), { prefix: '/media', setHeaders: staticFileHeaders });

  if (process.env.NODE_ENV !== 'production') {
    const document = SwaggerModule.createDocument(
      app,
      new DocumentBuilder().setTitle('PhotographerApp API').addBearerAuth().build(),
    );
    SwaggerModule.setup('swagger', app, document);
  }

  const port = configService.get<number>('port')!;
  await app.listen(port, '0.0.0.0');
}

await bootstrap();
