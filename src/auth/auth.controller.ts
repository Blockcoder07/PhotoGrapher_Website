import { Body, Controller, Get, NotFoundException, Post, Req, UnauthorizedException, UseGuards } from '@nestjs/common';
import { AuthService } from './auth.service.js';
import { LoginRequestDto } from './dto/login-request.dto.js';
import { RefreshTokenRequestDto } from './dto/refresh-token-request.dto.js';
import { ChangePasswordRequestDto } from './dto/change-password-request.dto.js';
import { ApiResponse } from '../common/dto/api-response.dto.js';
import { JwtAuthGuard } from './jwt-auth.guard.js';
import { JwtPayload } from './jwt-payload.js';

@Controller('api/auth')
export class AuthController {
  constructor(private readonly authService: AuthService) {}

  @Post('login')
  async login(@Body() dto: LoginRequestDto) {
    const result = await this.authService.login(dto);
    return ApiResponse.ok('Login successful', result);
  }

  @Post('refresh')
  async refresh(@Body() dto: RefreshTokenRequestDto) {
    const result = await this.authService.refreshAccessToken(dto.refreshToken);
    if (!result) throw new UnauthorizedException('Invalid refresh token');

    return ApiResponse.ok('Token refreshed', {
      token: result.token,
      refreshToken: dto.refreshToken,
      expiresAt: result.expiresAt,
    });
  }

  @UseGuards(JwtAuthGuard)
  @Post('change-password')
  async changePassword(@Req() req: { user: JwtPayload }, @Body() dto: ChangePasswordRequestDto) {
    const errors = await this.authService.changePassword(req.user.sub, dto.currentPassword, dto.newPassword);
    if (errors.length > 0) {
      return ApiResponse.fail('Password change failed', errors);
    }
    return ApiResponse.ok('Password changed successfully');
  }

  @UseGuards(JwtAuthGuard)
  @Get('me')
  async getCurrentUser(@Req() req: { user: JwtPayload }) {
    const user = await this.authService.getCurrentUser(req.user.sub);
    if (!user) throw new NotFoundException('User not found');
    return ApiResponse.ok('User retrieved', user);
  }
}
