import { SetMetadata } from '@nestjs/common';

export const ROLES_KEY = 'roles';
/** Mirrors ASP.NET's `[Authorize(Roles = "...")]`. */
export const Roles = (...roles: string[]) => SetMetadata(ROLES_KEY, roles);
