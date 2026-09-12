export class UserDto {
  id!: string;
  email!: string;
  userName!: string;
}

export class LoginResponseDto {
  token!: string;
  refreshToken!: string;
  expiresAt!: Date;
  user!: UserDto;
}
