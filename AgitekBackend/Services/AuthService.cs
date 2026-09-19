using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Common.Utils;
using AgitekBackend.DTOs.Auths;
using AgitekBackend.DTOs.Users;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace AgitekBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IDatabase _redis;

        public AuthService(
            IUserService userService,
            IEmailService emailService,
            IConfiguration config,
            IConnectionMultiplexer redis)
        {
            _userService = userService;
            _emailService = emailService;
            _config = config;
            _redis = redis.GetDatabase();
        }

        public async Task<TokenResponseDTO> LoginAsync(SignInDTO dto)
        {
            var user = await ValidateCredentialsAsync(dto.UsernameOrEmail, dto.Password);
            await _userService.AddProviderAsync(user.Id, Provider.Credentials);
            return await CreateTokenPairAsync(user);
        }

        public async Task<TokenResponseDTO> RegisterAsync(SignUpDTO dto)
        {
            var existingByEmail = await _userService.GetByEmailAsync(dto.Email);
            if (existingByEmail != null)
                throw new BadRequestException("Email already in use.", "EMAIL_ALREADY_EXISTS");

            var existingByUsername = await _userService.GetByUsernameOrEmailAsync(dto.Username);
            if (existingByUsername != null)
                throw new BadRequestException("Username already taken.", "USERNAME_ALREADY_EXISTS");

            var createUserDto = new CreateUserDTO
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password,
                Role = RoleName.Customer,
                Profile = dto.Profile ?? new DTOs.Profiles.CreateProfileDTO { FullName = dto.Username }
            };

            var userDto = await _userService.CreateAsync(createUserDto);
            var user = await _userService.GetRawByIdAsync(userDto.Id) 
                ?? throw new NotFoundException("User not found after creation.");

            await _userService.AddProviderAsync(user.Id, Provider.Credentials);

            _ = SendVerificationEmailAsync(user);

            return await CreateTokenPairAsync(user);
        }

        public async Task<TokenResponseDTO> VerifyAccountAsync(string token, string userId)
        {
            var storedUserId = await _redis.StringGetAsync($"verify:{token}");
            if (!storedUserId.HasValue || storedUserId.ToString() != userId)
                throw new UnauthorizedException("Invalid or expired verification token.", "INVALID_TOKEN");

            await _userService.UpdateVerifyStatusAsync(int.Parse(userId), true);
            await _redis.KeyDeleteAsync($"verify:{token}");

            var user = await _userService.GetRawByIdAsync(int.Parse(userId))
                ?? throw new NotFoundException("User not found.");

            return await CreateTokenPairAsync(user);
        }

        public async Task ResendVerifyCodeAsync(string userId)
        {
            var user = await _userService.GetRawByIdAsync(int.Parse(userId))
                ?? throw new NotFoundException("User not found.", "USER_NOT_FOUND");

            await SendVerificationEmailAsync(user);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userService.GetByEmailAsync(email)
                ?? throw new NotFoundException("User with this email does not exist.", "USER_NOT_FOUND");

            var oldToken = await _redis.StringGetAsync($"forgot-password-user:{user.Id}");
            if (oldToken.HasValue)
            {
                await _redis.KeyDeleteAsync($"forgot-password:{oldToken}");
                await _redis.KeyDeleteAsync($"forgot-password-user:{user.Id}");
            }

            var token = AuthUtils.GenerateUuid();
            var frontendUrl = _config["Frontend:Url"] ?? "http://localhost:3000";
            var resetLink = $"{frontendUrl}/reset-password?token={token}";
            var ttl = TimeSpan.FromSeconds(300);

            await _redis.StringSetAsync($"forgot-password:{token}", user.Id.ToString(), ttl);
            await _redis.StringSetAsync($"forgot-password-user:{user.Id}", token, ttl);

            await _emailService.SendForgotPasswordEmailAsync(user.Email, user.Username, resetLink);
        }

        public async Task ResetPasswordAsync(ResetPasswordDTO dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new BadRequestException("Passwords do not match.", "PASSWORD_MISMATCH");

            var storedUserId = await _redis.StringGetAsync($"forgot-password:{dto.Token}");
            if (!storedUserId.HasValue)
                throw new UnauthorizedException("Invalid or expired reset token.", "INVALID_TOKEN");

            var userId = int.Parse(storedUserId.ToString());
            await _userService.ChangePasswordAsync(userId, dto.Password);

            await _redis.KeyDeleteAsync($"forgot-password:{dto.Token}");
            await _redis.KeyDeleteAsync($"forgot-password-user:{userId}");
        }

        public async Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken)
        {
            var storedUserId = await _redis.StringGetAsync($"refresh:{refreshToken}");
            if (!storedUserId.HasValue)
                throw new UnauthorizedException("Invalid or expired refresh token.", "REFRESH_TOKEN_INVALID");

            var user = await _userService.GetRawByIdAsync(int.Parse(storedUserId.ToString()));
            if (user == null || !user.IsActive)
                throw new UnauthorizedException("This account has been locked or not found.", "ACCOUNT_LOCKED");

            await _redis.KeyDeleteAsync($"refresh:{refreshToken}");
            return await CreateTokenPairAsync(user);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await _redis.KeyDeleteAsync($"refresh:{refreshToken}");
        }

        public async Task<TokenResponseDTO> HandleGoogleLoginAsync(string email, string name)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                var safeUsername = await GenerateSafeUsernameAsync(name);
                var createUserDto = new CreateUserDTO
                {
                    Username = safeUsername,
                    Email = email,
                    Password = AuthUtils.GenerateUuid(),
                    Role = RoleName.Customer,
                    Profile = new DTOs.Profiles.CreateProfileDTO { FullName = name }
                };

                var userDto = await _userService.CreateAsync(createUserDto);
                await _userService.UpdateVerifyStatusAsync(userDto.Id, true);
                user = await _userService.GetRawByIdAsync(userDto.Id);
            }

            if (user != null)
            {
                await _userService.AddProviderAsync(user.Id, Provider.Google);
            }

            return await CreateTokenPairAsync(user!);
        }

        private async Task<User> ValidateCredentialsAsync(string usernameOrEmail, string password)
        {
            var user = await _userService.GetByUsernameOrEmailAsync(usernameOrEmail);

            if (user == null || !AuthUtils.VerifyPassword(password, user.PasswordHash))
                throw new UnauthorizedException("Invalid credentials.", "INVALID_CREDENTIALS");

            if (!user.IsActive)
                throw new UnauthorizedException("This account has been deactivated.", "ACCOUNT_LOCKED");

            return user;
        }

        private async Task<TokenResponseDTO> CreateTokenPairAsync(User user)
        {
            var secretKey = _config["JwtSettings:SecretKey"]!;
            var issuer = _config["JwtSettings:Issuer"]!;
            var audience = _config["JwtSettings:Audience"]!;
            var expiryMin = int.Parse(_config["JwtSettings:ExpiryInMinutes"] ?? "15");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("isVerified", user.IsVerified.ToString()),
                new Claim("isActive", user.IsActive.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMin),
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = AuthUtils.GenerateUuid();
            var refreshTtlSeconds = int.Parse(_config["JwtSettings:RefreshTokenTtlSeconds"] ?? "604800");
            
            await _redis.StringSetAsync(
                $"refresh:{refreshToken}",
                user.Id.ToString(),
                TimeSpan.FromSeconds(refreshTtlSeconds));

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        private async Task SendVerificationEmailAsync(User user)
        {
            var token = AuthUtils.GenerateUuid();
            await _redis.StringSetAsync($"verify:{token}", user.Id.ToString(), TimeSpan.FromSeconds(300));
            await _emailService.SendVerificationEmailAsync(user.Email, user.Username, token);
        }

        private async Task<string> GenerateSafeUsernameAsync(string displayName)
        {
            var baseUsername = new string(
                displayName.ToLowerInvariant()
                           .Replace(" ", "_")
                           .Where(c => char.IsLetterOrDigit(c) || c == '_')
                           .ToArray());

            if (baseUsername.Length < 6)
                baseUsername = baseUsername.PadRight(6, '_');
            if (baseUsername.Length > 16)
                baseUsername = baseUsername[..16];

            var candidate = baseUsername;
            var existing = await _userService.GetByUsernameOrEmailAsync(candidate);
            if (existing != null)
                candidate = $"{baseUsername}_{new Random().Next(1000, 9999)}";

            return candidate;
        }
    }
}
