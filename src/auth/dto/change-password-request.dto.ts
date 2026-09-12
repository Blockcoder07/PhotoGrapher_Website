import { IsNotEmpty, Matches, MinLength } from 'class-validator';
import { IsDifferentFrom } from '../../common/validators/is-different-from.js';

export class ChangePasswordRequestDto {
  @IsNotEmpty({ message: 'Current password is required' })
  currentPassword!: string;

  @IsNotEmpty({ message: 'New password is required' })
  @MinLength(8, { message: 'Password must be at least 8 characters' })
  @Matches(/[A-Z]/, { message: 'Password must contain at least one uppercase letter' })
  @Matches(/[a-z]/, { message: 'Password must contain at least one lowercase letter' })
  @Matches(/[0-9]/, { message: 'Password must contain at least one digit' })
  @Matches(/[^a-zA-Z0-9]/, { message: 'Password must contain at least one special character' })
  @IsDifferentFrom('currentPassword', { message: 'New password must be different from current password' })
  newPassword!: string;
}
