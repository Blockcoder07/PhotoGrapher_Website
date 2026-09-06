using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotographerApp.Core.DTOs;
using PhotographerApp.Core.Interfaces;

namespace PhotographerApp.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<IdentityUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse { Success = false, Message = "Invalid request" });

        // Demo login for development
        if (request.Email == "admin@photographer.com" && request.Password == "admin123")
        {
            var demoUser = new IdentityUser
            {
                Id = "demo-admin-id",
                Email = request.Email,
                UserName = "admin",
                EmailConfirmed = true
            };
            var (accessToken, refreshToken, expiresAt) = await _tokenService.GenerateTokensAsync(demoUser, new[] { "Admin" });

            var response = new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    Id = demoUser.Id,
                    Email = request.Email,
                    UserName = "admin"
                }
            };

            return Ok(new ApiResponse<LoginResponse> { Success = true, Message = "Login successful", Data = response });
        }

        // Try Identity user login as fallback
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new ApiResponse { Success = false, Message = "Invalid email or password" });

        var (token, refresh, expires) = await _tokenService.GenerateTokensAsync(user);

        var loginResponse = new LoginResponse
        {
            Token = token,
            RefreshToken = refresh,
            ExpiresAt = expires,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? ""
            }
        };

        return Ok(new ApiResponse<LoginResponse> { Success = true, Message = "Login successful", Data = loginResponse });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var newAccessToken = await _tokenService.RefreshAccessTokenAsync(request.RefreshToken);
        if (newAccessToken == null)
            return Unauthorized(new ApiResponse { Success = false, Message = "Invalid refresh token" });

        var expiryMinutes = 60;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var response = new LoginResponse
        {
            Token = newAccessToken,
            RefreshToken = request.RefreshToken,
            ExpiresAt = expiresAt
        };

        return Ok(new ApiResponse<LoginResponse> { Success = true, Message = "Token refreshed", Data = response });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse { Success = false, Message = "User not found" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new ApiResponse { Success = false, Message = "User not found" });

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new ApiResponse { Success = false, Message = "Password change failed", Errors = errors });
        }

        return Ok(new ApiResponse { Success = true, Message = "Password changed successfully" });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse { Success = false, Message = "User not found" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new ApiResponse { Success = false, Message = "User not found" });

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            UserName = user.UserName ?? ""
        };

        return Ok(new ApiResponse<UserDto> { Success = true, Message = "User retrieved", Data = userDto });
    }
}
