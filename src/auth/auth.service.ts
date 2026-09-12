import { randomBytes } from 'node:crypto';
import { Injectable, NotFoundException, UnauthorizedException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { JwtService } from '@nestjs/jwt';
import { PrismaService } from '../prisma/prisma.service.js';
import { hashPasswordIdentityV3, verifyIdentityPasswordHash } from './identity-password-hasher.js';
import { JwtPayload } from './jwt-payload.js';
import { LoginRequestDto } from './dto/login-request.dto.js';
import { LoginResponseDto, UserDto } from './dto/auth-response.dto.js';

interface TokenSubject {
  id: string;
  email: string;
  userName: string;
}

@Injectable()
export class AuthService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly jwtService: JwtService,
    private readonly configService: ConfigService,
  ) {}

  async login(dto: LoginRequestDto): Promise<LoginResponseDto> {
    // Demo login for development — mirrors AuthController.cs's hardcoded bypass.
    if (dto.email === 'admin@photographer.com' && dto.password === 'admin123') {
      return this.issueTokens({ id: 'demo-admin-id', email: dto.email, userName: 'admin' }, ['Admin']);
    }

    const user = await this.prisma.user.findFirst({
      where: { normalizedEmail: dto.email.toUpperCase() },
    });
    if (!user?.passwordHash || !verifyIdentityPasswordHash(user.passwordHash, dto.password)) {
      throw new UnauthorizedException('Invalid email or password');
    }

    const roles = await this.getRoleNames(user.id);
    return this.issueTokens({ id: user.id, email: user.email ?? '', userName: user.userName ?? '' }, roles);
  }

  /**
   * Ported as-is from TokenService.cs: the "refresh token" issued at login is random bytes, not a
   * JWT, so validating it here as a JWT always fails. Confirmed the client never calls this endpoint
   * (client/src/services/api.ts has no refresh call) — this is a pre-existing dead/broken feature,
   * not something introduced by the migration.
   */
  async refreshAccessToken(refreshToken: string): Promise<{ token: string; expiresAt: Date } | null> {
    try {
      const payload = this.jwtService.verify<JwtPayload>(refreshToken);
      return this.buildAccessToken(
        { id: payload.sub, email: payload.email, userName: payload.userName },
        payload.roles,
      );
    } catch {
      return null;
    }
  }

  async changePassword(userId: string, currentPassword: string, newPassword: string): Promise<string[]> {
    const user = await this.prisma.user.findUnique({ where: { id: userId } });
    if (!user) throw new NotFoundException('User not found');

    if (!user.passwordHash || !verifyIdentityPasswordHash(user.passwordHash, currentPassword)) {
      return ['Incorrect password.'];
    }

    await this.prisma.user.update({
      where: { id: userId },
      data: { passwordHash: hashPasswordIdentityV3(newPassword) },
    });
    return [];
  }

  async getCurrentUser(userId: string): Promise<UserDto | null> {
    const user = await this.prisma.user.findUnique({ where: { id: userId } });
    if (!user) return null;
    return { id: user.id, email: user.email ?? '', userName: user.userName ?? '' };
  }

  private async getRoleNames(userId: string): Promise<string[]> {
    const userRoles = await this.prisma.userRole.findMany({
      where: { userId },
      include: { role: true },
    });
    return userRoles.map((ur) => ur.role.name).filter((name): name is string => !!name);
  }

  private buildAccessToken(subject: TokenSubject, roles?: string[]): { token: string; expiresAt: Date } {
    const expiryMinutes = this.configService.get<number>('jwt.expiryMinutes')!;
    const expiresAt = new Date(Date.now() + expiryMinutes * 60_000);
    const payload: JwtPayload = {
      sub: subject.id,
      email: subject.email,
      userName: subject.userName,
      roles: roles ?? [],
    };
    const token = this.jwtService.sign(payload, { expiresIn: `${expiryMinutes}m` });
    return { token, expiresAt };
  }

  private issueTokens(subject: TokenSubject, roles: string[]): LoginResponseDto {
    const { token, expiresAt } = this.buildAccessToken(subject, roles);
    const refreshToken = randomBytes(32).toString('base64');

    return {
      token,
      refreshToken,
      expiresAt,
      user: { id: subject.id, email: subject.email, userName: subject.userName },
    };
  }
}
