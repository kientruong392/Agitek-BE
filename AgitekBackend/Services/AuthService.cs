using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Common.Utils;
using AgitekBackend.DTOs.Auths;
using AgitekBackend.DTOs.Profiles;
using AgitekBackend.DTOs.Users;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public AuthService(
            IUserService userService,
            IEmailService emailService,
            IConfiguration config,
            IConnectionMultiplexer redis,
            IMapper mapper)
        {
            _userService = userService;
            _emailService = emailService;
            _config = config;
            _redis = redis.GetDatabase();
            _mapper = mapper;
        }
        private async Task<string> GenerateSafeUsernameAsync(string email)
        {
            var emailPrefix = email.Split('@')[0].ToLowerInvariant();
            var cleanPrefix = Regex.Replace(emailPrefix, @"[^a-zA-Z0-9]", "");
            if (cleanPrefix.Length > 14) cleanPrefix = cleanPrefix[..14];
            string username;
            do
            {
                var random = Random.Shared.Next(10000, 99999);
                username = $"{cleanPrefix}_{random}";
            }
            while (await _userService.GetByUsernameOrEmailAsync(username) != null);
            return username;
        }

        public async Task<TokenResponseDTO> LoginAsync(SignInDTO dto)
        {
            var userDto = await ValidateCredentialsAsync(dto.UsernameOrEmail, dto.Password);
            await _userService.AddProviderAsync(userDto.Id, Provider.Credentials);
            return await CreateTokenPairAsync(userDto);
        }

        public async Task<UserDTO> CreateGuestUserAsync(CreateGuestDTO dto)
        {
            var createUserDto = new CreateUserDTO
            {
                Username = null,
                Email = null,
                Password = null,
                Role = RoleName.Guest,
                Profile = new CreateProfileDTO()
                {
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                }
            };
            return await _userService.CreateAsync(createUserDto);
        }

        public async Task<TokenResponseDTO> RegisterAsync(SignUpDTO dto)
        {
            var existingByEmail = await _userService.GetByUsernameOrEmailAsync(dto.Email);
            if (existingByEmail != null)
                throw new BadRequestException("Email already in use", "EMAIL_ALREADY_EXISTS");

            var existingByUsername = await _userService.GetByUsernameOrEmailAsync(dto.Username);
            if (existingByUsername != null)
                throw new BadRequestException("Username already taken", "USERNAME_ALREADY_EXISTS");

            var createUserDto = _mapper.Map<CreateUserDTO>(dto);

            var userDto = await _userService.CreateAsync(createUserDto);

            await _userService.AddProviderAsync(userDto.Id, Provider.Credentials);

            await SendVerificationEmailAsync(userDto);

            return await CreateTokenPairAsync(userDto);
        }

        public async Task<TokenResponseDTO> VerifyAccountAsync(string token, string userId)
        {
            var storedUserId = await _redis.StringGetAsync($"verify:{token}");
            if (!storedUserId.HasValue || storedUserId.ToString() != userId)
                throw new UnauthorizedException("Invalid or expired verification token", "INVALID_TOKEN");

            await _userService.UpdateVerifyStatusAsync(int.Parse(userId), true);
            await _redis.KeyDeleteAsync($"verify:{token}");

            var user = await _userService.GetByIdAsync(int.Parse(userId))
                ?? throw new NotFoundException("User not found");

            return await CreateTokenPairAsync(user);
        }

        public async Task ResendVerifyCodeAsync(string userId)
        {
            var userDto = await _userService.GetByIdAsync(int.Parse(userId));
            await SendVerificationEmailAsync(userDto);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var userDto = await _userService.GetByEmailAsync(email);

            var oldToken = await _redis.StringGetAsync($"forgot-password-user:{userDto.Id}");
            if (oldToken.HasValue)
            {
                await _redis.KeyDeleteAsync($"forgot-password:{oldToken}");
                await _redis.KeyDeleteAsync($"forgot-password-user:{userDto.Id}");
            }

            var token = AuthUtils.GenerateUuid();
            var frontendUrl = _config["Frontend:Url"];
            var resetLink = $"{frontendUrl}/reset-password?token={token}";
            var ttl = TimeSpan.FromSeconds(300);

            await _redis.StringSetAsync($"forgot-password:{token}", userDto.Id.ToString(), ttl);
            await _redis.StringSetAsync($"forgot-password-user:{userDto.Id}", token, ttl);

            await _emailService.SendForgotPasswordEmailAsync(userDto.Email, userDto.Username, resetLink);
        }
        
        public async Task ResetPasswordAsync(ResetPasswordDTO dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new BadRequestException("Passwords do not match", "PASSWORD_MISMATCH");

            var storedUserId = await _redis.StringGetAsync($"forgot-password:{dto.Token}");
            if (!storedUserId.HasValue)
                throw new UnauthorizedException("Invalid or expired reset token", "INVALID_TOKEN");

            var userId = int.Parse(storedUserId.ToString());
            await _userService.ChangePasswordAsync(userId, dto.Password);

            await _redis.KeyDeleteAsync($"forgot-password:{dto.Token}");
            await _redis.KeyDeleteAsync($"forgot-password-user:{userId}");
        }

        private sealed record RotatedRefreshRecord(int UserId, string AccessToken, string RefreshToken);
        private static readonly TimeSpan RefreshRotationGraceWindow = TimeSpan.FromMinutes(5);

        public async Task SetPasswordAsync(int userId, SetPasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6 || dto.Password.Length > 100)
                throw new BadRequestException("Password must be between 6 and 100 characters", "INVALID_PASSWORD");

            if (dto.Password != dto.ConfirmPassword)
                throw new BadRequestException("Passwords do not match", "PASSWORD_MISMATCH");

            var user = await _userService.GetByIdAsync(userId);
            if (user.HasPassword)
                throw new BadRequestException("This account already has a password", "PASSWORD_ALREADY_SET");

            await _userService.ChangePasswordAsync(userId, dto.Password);
            await _userService.AddProviderAsync(userId, Provider.Credentials);
        }

        public async Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new UnauthorizedException("Invalid or expired refresh token", "REFRESH_TOKEN_INVALID");

            var refreshKey = $"refresh:{refreshToken}";
            var rotatedKey = $"refresh-rotated:{refreshToken}";
            var lockKey = $"refresh-lock:{refreshToken}";

            // 1. Idempotent grace window: if this refresh token was just rotated by a concurrent
            //    request (e.g. parallel Server Actions / RSC / middleware), return the same pair.
            var cachedRotated = await TryGetRotatedTokensAsync(rotatedKey);
            if (cachedRotated != null)
                return cachedRotated;

            // 2. Acquire short distributed lock per refresh token so concurrent requests don't race.
            var lockValue = Guid.NewGuid().ToString("N");
            var lockAcquired = false;
            for (var attempt = 0; attempt < 25; attempt++)
            {
                lockAcquired = await _redis.StringSetAsync(lockKey, lockValue, TimeSpan.FromSeconds(10), When.NotExists);
                if (lockAcquired) break;

                await Task.Delay(80);
                cachedRotated = await TryGetRotatedTokensAsync(rotatedKey);
                if (cachedRotated != null)
                    return cachedRotated;
            }

            try
            {
                cachedRotated = await TryGetRotatedTokensAsync(rotatedKey);
                if (cachedRotated != null)
                    return cachedRotated;

                var storedUserId = await _redis.StringGetAsync(refreshKey);
                if (!storedUserId.HasValue)
                    throw new UnauthorizedException("Invalid or expired refresh token", "REFRESH_TOKEN_INVALID");

                var user = await _userService.GetByIdAsync(int.Parse(storedUserId.ToString()));
                if (user == null || !user.IsActive)
                    throw new UnauthorizedException("This account has been locked or not found", "ACCOUNT_LOCKED");

                var newTokens = await CreateTokenPairAsync(user);

                var recordJson = JsonSerializer.Serialize(new RotatedRefreshRecord(
                    user.Id,
                    newTokens.AccessToken,
                    newTokens.RefreshToken));
                await _redis.StringSetAsync(rotatedKey, recordJson, RefreshRotationGraceWindow);
                await _redis.KeyDeleteAsync(refreshKey);

                return newTokens;
            }
            finally
            {
                if (lockAcquired)
                {
                    var currentLock = await _redis.StringGetAsync(lockKey);
                    if (currentLock == lockValue)
                        await _redis.KeyDeleteAsync(lockKey);
                }
            }
        }

        private async Task<TokenResponseDTO?> TryGetRotatedTokensAsync(string rotatedKey)
        {
            var rotatedRaw = await _redis.StringGetAsync(rotatedKey);
            if (!rotatedRaw.HasValue) return null;

            var record = JsonSerializer.Deserialize<RotatedRefreshRecord>(rotatedRaw.ToString());
            if (record == null) return null;

            var user = await _userService.GetByIdAsync(record.UserId);
            if (user == null || !user.IsActive)
                throw new UnauthorizedException("This account has been locked or not found", "ACCOUNT_LOCKED");

            return new TokenResponseDTO
            {
                AccessToken = record.AccessToken,
                RefreshToken = record.RefreshToken,
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;

            var rotatedKey = $"refresh-rotated:{refreshToken}";
            var rotatedRaw = await _redis.StringGetAsync(rotatedKey);
            if (rotatedRaw.HasValue)
            {
                var record = JsonSerializer.Deserialize<RotatedRefreshRecord>(rotatedRaw.ToString());
                if (record != null && !string.IsNullOrWhiteSpace(record.RefreshToken))
                    await _redis.KeyDeleteAsync($"refresh:{record.RefreshToken}");
                await _redis.KeyDeleteAsync(rotatedKey);
            }

            await _redis.KeyDeleteAsync($"refresh:{refreshToken}");
        }

        public async Task<TokenResponseDTO> HandleGoogleLoginAsync(string email, string name)
        {
            var userDto = await _userService.CheckEmailExists(email);
            if (userDto == null)
            {
                var safeUsername = await GenerateSafeUsernameAsync(email);
                var createUserDto = new CreateUserDTO
                {
                    Username = safeUsername,
                    Email = email,
                    Password = null,
                    Role = RoleName.Customer,
                    Profile = new CreateProfileDTO()
                    {
                        FullName = name
                    }
                };

                userDto = await _userService.CreateAsync(createUserDto);
            }

            if (!userDto.IsVerified)
            {
                await _userService.UpdateVerifyStatusAsync(userDto.Id, true);
                userDto.IsVerified = true;
            }

            await _userService.AddProviderAsync(userDto.Id, Provider.Google);

            return await CreateTokenPairAsync(userDto);
        }

        private async Task<UserDTO> ValidateCredentialsAsync(string usernameOrEmail, string password)
        {
            var user = await _userService.GetByUsernameOrEmailAsync(usernameOrEmail);
            if (user == null)
                throw new UnauthorizedException("Invalid credentials", "INVALID_CREDENTIALS");
            if (string.IsNullOrEmpty(user.PasswordHash))
                throw new UnauthorizedException("Invalid credentials", "INVALID_CREDENTIALS");
            if (!AuthUtils.VerifyPassword(password, user.PasswordHash))
                throw new UnauthorizedException("Invalid credentials", "INVALID_CREDENTIALS");
            if (!user.IsActive)
                throw new UnauthorizedException("This account has been deactivated", "ACCOUNT_LOCKED");

            return _mapper.Map<UserDTO>(user);
        }

        private async Task<TokenResponseDTO> CreateTokenPairAsync(UserDTO userDto)
        {
            var secretKey = _config["JwtSettings:SecretKey"]!;
            var issuer = _config["JwtSettings:Issuer"]!;
            var audience = _config["JwtSettings:Audience"]!;
            var expiryMin = int.Parse(_config["JwtSettings:ExpiryInMinutes"] ?? "15");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userDto.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, userDto.Email),
                new("username", userDto.Username),
                new("fullname", userDto.Profile?.FullName ?? userDto.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("isVerified", userDto.IsVerified ? "true" : "false"),
                new("isActive", userDto.IsActive ? "true" : "false"),
                new("role", userDto.Role.ToString()),
            };

            if (!string.IsNullOrWhiteSpace(userDto.Profile?.AvatarUrl))
            {
                claims.Add(new Claim("avatarUrl", userDto.Profile.AvatarUrl));
            }

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
                userDto.Id.ToString(),
                TimeSpan.FromSeconds(refreshTtlSeconds));

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        private async Task SendVerificationEmailAsync(UserDTO userDto)
        {
            var token = AuthUtils.GenerateUuid();
            await _redis.StringSetAsync($"verify:{token}", userDto.Id.ToString(), TimeSpan.FromSeconds(300));
            await _emailService.SendVerificationEmailAsync(userDto.Email, userDto.Username, token);
        }
    }
}