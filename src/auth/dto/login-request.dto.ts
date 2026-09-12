import { IsEmail, IsNotEmpty, MinLength } from 'class-validator';

export class LoginRequestDto {
  @IsNotEmpty({ message: 'Email is required' })
  @IsEmail({}, { message: 'Email format is invalid' })
  email!: string;

  @IsNotEmpty({ message: 'Password is required' })
  @MinLength(8, { message: 'Password must be at least 8 characters' })
  password!: string;
}
