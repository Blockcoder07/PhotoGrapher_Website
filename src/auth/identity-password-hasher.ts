import { pbkdf2Sync, randomBytes, timingSafeEqual } from 'node:crypto';

/**
 * Compatible with ASP.NET Core Identity's `PasswordHasher<TUser>` (v2 and v3 formats), since the
 * AspNetUsers table (and any password hash already stored in it) is shared with the .NET app.
 * v3 layout: 0x01 marker, prf (u32 BE), iterations (u32 BE), saltSize (u32 BE), salt, subkey.
 * v2 layout: 0x00 marker, fixed SHA1/1000 iterations/16-byte salt/32-byte subkey, no header fields.
 */

function prfToDigest(prf: number): 'sha1' | 'sha256' | 'sha512' {
  switch (prf) {
    case 0:
      return 'sha1';
    case 1:
      return 'sha256';
    case 2:
      return 'sha512';
    default:
      throw new Error(`Unsupported ASP.NET Identity PRF id: ${prf}`);
  }
}

export function verifyIdentityPasswordHash(base64Hash: string, password: string): boolean {
  let hashBytes: Buffer;
  try {
    hashBytes = Buffer.from(base64Hash, 'base64');
  } catch {
    return false;
  }
  if (hashBytes.length === 0) return false;

  const formatMarker = hashBytes[0];
  const passwordBytes = Buffer.from(password, 'utf8');

  try {
    if (formatMarker === 0x00) {
      if (hashBytes.length !== 1 + 16 + 32) return false;
      const salt = hashBytes.subarray(1, 17);
      const expectedSubkey = hashBytes.subarray(17);
      const actualSubkey = pbkdf2Sync(passwordBytes, salt, 1000, expectedSubkey.length, 'sha1');
      return timingSafeEqual(actualSubkey, expectedSubkey);
    }

    if (formatMarker === 0x01) {
      if (hashBytes.length < 13) return false;
      const prf = hashBytes.readUInt32BE(1);
      const iterCount = hashBytes.readUInt32BE(5);
      const saltSize = hashBytes.readUInt32BE(9);
      if (hashBytes.length < 13 + saltSize) return false;
      const salt = hashBytes.subarray(13, 13 + saltSize);
      const expectedSubkey = hashBytes.subarray(13 + saltSize);
      const digest = prfToDigest(prf);
      const actualSubkey = pbkdf2Sync(passwordBytes, salt, iterCount, expectedSubkey.length, digest);
      return timingSafeEqual(actualSubkey, expectedSubkey);
    }
  } catch {
    return false;
  }

  return false;
}

/** Always writes v3 (HMACSHA256, 100_000 iterations, 16-byte salt, 32-byte subkey) — same as a fresh ASP.NET Core Identity UserManager hash. */
export function hashPasswordIdentityV3(password: string): string {
  const iterCount = 100_000;
  const saltSize = 16;
  const subkeySize = 32;
  const salt = randomBytes(saltSize);
  const subkey = pbkdf2Sync(Buffer.from(password, 'utf8'), salt, iterCount, subkeySize, 'sha256');

  const output = Buffer.alloc(13 + saltSize + subkeySize);
  output[0] = 0x01;
  output.writeUInt32BE(1, 1);
  output.writeUInt32BE(iterCount, 5);
  output.writeUInt32BE(saltSize, 9);
  salt.copy(output, 13);
  subkey.copy(output, 13 + saltSize);

  return output.toString('base64');
}
