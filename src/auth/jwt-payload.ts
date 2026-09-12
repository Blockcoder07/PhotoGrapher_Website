export interface JwtPayload {
  sub: string;
  email: string;
  userName: string;
  roles: string[];
}
