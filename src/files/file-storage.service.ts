import { randomUUID } from 'node:crypto';
import * as fs from 'node:fs/promises';
import * as path from 'node:path';
import { Injectable, OnModuleInit } from '@nestjs/common';
import sharp from 'sharp';

const MAX_IMAGE_SIZE_BYTES = 10 * 1024 * 1024;
const MAX_VIDEO_SIZE_BYTES = 200 * 1024 * 1024;
const THUMBNAIL_WIDTH = 400;

const ALLOWED_IMAGE_EXTENSIONS = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.tiff', '.tif', '.webp', '.ico', '.svg', '.heic', '.heif'];
const ALLOWED_VIDEO_EXTENSIONS = ['.mp4', '.webm', '.mov', '.avi', '.mkv', '.flv', '.wmv'];

const IMAGE_MAGIC_NUMBERS: number[][] = [
  [0xff, 0xd8, 0xff], // JPEG
  [0x89, 0x50, 0x4e, 0x47], // PNG
  [0x47, 0x49, 0x46, 0x38], // GIF
  [0x42, 0x4d], // BMP
  [0x49, 0x49, 0x2a, 0x00], // TIFF (little-endian)
  [0x4d, 0x4d, 0x00, 0x2a], // TIFF (big-endian)
  [0x52, 0x49, 0x46, 0x46], // WEBP
  [0x00, 0x00, 0x01, 0x00], // ICO
  [0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70], // HEIC/HEIF
];
const VIDEO_MAGIC_NUMBERS: number[][] = [
  [0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70], // MP4
  [0x1a, 0x45, 0xdf, 0xa3], // MKV
  [0x52, 0x49, 0x46, 0x46], // AVI/WEBM
  [0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70], // MOV
];

export interface UploadedFileLike {
  buffer: Buffer;
  originalname: string;
  size: number;
}

@Injectable()
export class FileStorageService implements OnModuleInit {
  private readonly basePath: string;

  constructor() {
    this.basePath = path.join(process.cwd(), 'uploads');
  }

  async onModuleInit(): Promise<void> {
    await Promise.all(
      ['photos', 'videos', 'team'].map((dir) => fs.mkdir(path.join(this.basePath, dir), { recursive: true })),
    );
  }

  async savePhoto(file: UploadedFileLike): Promise<string> {
    const { fileName, dir, yearMonth } = await this.preparePath(file.originalname, 'photos');
    await fs.writeFile(path.join(dir, fileName), file.buffer);
    return `/uploads/photos/${yearMonth}/${fileName}`;
  }

  async savePhotoThumbnail(file: UploadedFileLike): Promise<string> {
    const { fileName, dir, yearMonth } = await this.preparePath(file.originalname, 'photos');
    const thumbnailFileName = `${path.parse(fileName).name}_thumb.jpg`;

    const image = sharp(file.buffer);
    const metadata = await image.metadata();
    const aspectRatio = (metadata.width ?? THUMBNAIL_WIDTH) / (metadata.height ?? THUMBNAIL_WIDTH);
    const newHeight = Math.round(THUMBNAIL_WIDTH / aspectRatio);

    await image.resize(THUMBNAIL_WIDTH, newHeight).jpeg().toFile(path.join(dir, thumbnailFileName));

    return `/uploads/photos/${yearMonth}/${thumbnailFileName}`;
  }

  async getImageDimensions(buffer: Buffer): Promise<{ width: number; height: number }> {
    const metadata = await sharp(buffer).metadata();
    return { width: metadata.width ?? 0, height: metadata.height ?? 0 };
  }

  async saveVideo(file: UploadedFileLike): Promise<string> {
    const { fileName, dir, yearMonth } = await this.preparePath(file.originalname, 'videos');
    await fs.writeFile(path.join(dir, fileName), file.buffer);
    return `/uploads/videos/${yearMonth}/${fileName}`;
  }

  /** Mirrors FileStorageService.cs's SaveVideoThumbnailAsync: it tries to decode the video bytes as
   * a still image (ImageSharp can't handle video codecs), which fails for real videos and returns
   * "" — a pre-existing no-op, not something this migration is meant to fix. */
  async saveVideoThumbnail(file: UploadedFileLike): Promise<string> {
    try {
      const { fileName, dir, yearMonth } = await this.preparePath(file.originalname, 'videos');
      const thumbnailFileName = `${path.parse(fileName).name}_thumb.jpg`;

      const image = sharp(file.buffer);
      const metadata = await image.metadata();
      const aspectRatio = (metadata.width ?? THUMBNAIL_WIDTH) / (metadata.height ?? THUMBNAIL_WIDTH);
      const newHeight = Math.round(THUMBNAIL_WIDTH / aspectRatio);

      await image.resize(THUMBNAIL_WIDTH, newHeight).jpeg().toFile(path.join(dir, thumbnailFileName));
      return `/uploads/videos/${yearMonth}/${thumbnailFileName}`;
    } catch {
      return '';
    }
  }

  async deleteFile(filePath: string | null | undefined): Promise<void> {
    if (!filePath) return;
    const fullPath = path.join(this.basePath, filePath.replace(/^\/?uploads\//, ''));
    await fs.rm(fullPath, { force: true });
  }

  validateImageFile(file: UploadedFileLike): { valid: boolean; error?: string } {
    if (!file || file.size === 0) return { valid: false, error: 'File is empty' };
    if (file.size > MAX_IMAGE_SIZE_BYTES) {
      return { valid: false, error: `File size exceeds ${MAX_IMAGE_SIZE_BYTES / (1024 * 1024)}MB limit` };
    }

    const extension = path.extname(file.originalname).toLowerCase();
    if (!ALLOWED_IMAGE_EXTENSIONS.includes(extension)) {
      return {
        valid: false,
        error: 'File type not allowed. Allowed types: jpg, jpeg, png, gif, bmp, tiff, webp, ico, svg, heic, heif',
      };
    }

    if (!matchesMagicNumber(file.buffer, IMAGE_MAGIC_NUMBERS)) {
      return { valid: false, error: 'File content does not match image format' };
    }

    return { valid: true };
  }

  validateVideoFile(file: UploadedFileLike): { valid: boolean; error?: string } {
    if (!file || file.size === 0) return { valid: false, error: 'File is empty' };
    if (file.size > MAX_VIDEO_SIZE_BYTES) {
      return { valid: false, error: `File size exceeds ${MAX_VIDEO_SIZE_BYTES / (1024 * 1024)}MB limit` };
    }

    const extension = path.extname(file.originalname).toLowerCase();
    if (!ALLOWED_VIDEO_EXTENSIONS.includes(extension)) {
      return { valid: false, error: 'File type not allowed. Allowed types: mp4, webm, mov, avi, mkv, flv, wmv' };
    }

    if (!matchesMagicNumber(file.buffer, VIDEO_MAGIC_NUMBERS)) {
      return { valid: false, error: 'File content does not match video format' };
    }

    return { valid: true };
  }

  private async preparePath(
    originalFileName: string,
    kind: 'photos' | 'videos',
  ): Promise<{ fileName: string; dir: string; yearMonth: string }> {
    const fileName = generateFileName(originalFileName);
    const now = new Date();
    const yearMonth = `${now.getUTCFullYear()}/${String(now.getUTCMonth() + 1).padStart(2, '0')}`;
    const dir = path.join(this.basePath, kind, yearMonth);
    await fs.mkdir(dir, { recursive: true });
    return { fileName, dir, yearMonth };
  }
}

function generateFileName(originalFileName: string): string {
  return `${randomUUID().replace(/-/g, '')}${path.extname(originalFileName)}`;
}

function matchesMagicNumber(buffer: Buffer, allowed: number[][]): boolean {
  const header = buffer.subarray(0, 8);
  return allowed.some((magic) => magic.every((byte, i) => header[i] === byte));
}
