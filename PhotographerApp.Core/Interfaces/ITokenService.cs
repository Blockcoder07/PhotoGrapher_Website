using Microsoft.AspNetCore.Identity;

namespace PhotographerApp.Core.Interfaces;

public interface ITokenService
{
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> GenerateTokensAsync(IdentityUser user, IEnumerable<string>? overrideRoles = null);
    Task<string?> RefreshAccessTokenAsync(string refreshToken);
    bool ValidateToken(string token);
}
