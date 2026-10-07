using AgitekBackend.DTOs.Auths;
using AgitekBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgitekBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config      = config;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDTO>> Login([FromBody] SignInDTO dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDTO>> Register([FromBody] SignUpDTO dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }

        [HttpPost("verify-account")]
        public async Task<ActionResult<TokenResponseDTO>> VerifyAccount([FromBody] string token)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            var result = await _authService.VerifyAccountAsync(token, userId);
            return Ok(result);
        }

        [HttpPost("resend-verify-code")]
        public async Task<IActionResult> ResendVerifyCode()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("User ID not found in token");

            await _authService.ResendVerifyCodeAsync(userId);
            return Ok();
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok();
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok();
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDTO>> Refresh([FromBody] TokenResponseDTO body)
        {
            var result = await _authService.RefreshTokenAsync(body.RefreshToken);
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] TokenResponseDTO body)
        {
            await _authService.LogoutAsync(body.RefreshToken);
            return Ok();
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }

        [HttpGet("google")]
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), "Auth", null, Request.Scheme)
            };
            return Challenge(properties, "Google");
        }

        [HttpGet("google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded || result.Principal == null)
                return Unauthorized();

            var email = result.Principal.FindFirstValue(ClaimTypes.Email)
                ?? throw new InvalidOperationException("Google did not return an email");
            var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email;

            var tokens = await _authService.HandleGoogleLoginAsync(email, name);

            var frontendUrl = _config["Frontend:Url"] ?? "http://localhost:3000";
            return Redirect(
                $"{frontendUrl}/login/success?accessToken={tokens.AccessToken}&refreshToken={tokens.RefreshToken}");
        }
    }
}
