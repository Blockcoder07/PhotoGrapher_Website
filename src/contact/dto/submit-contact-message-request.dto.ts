import { IsEmail, IsNotEmpty, IsOptional, MaxLength } from 'class-validator';

export class SubmitContactMessageRequestDto {
  @IsNotEmpty({ message: 'Name is required' })
  @MaxLength(150)
  name!: string;

  @IsNotEmpty({ message: 'Email is required' })
  @IsEmail({}, { message: 'Email format is invalid' })
  email!: string;

  @IsOptional()
  @MaxLength(300)
  subject?: string;

  @IsNotEmpty({ message: 'Message is required' })
  message!: string;
}
