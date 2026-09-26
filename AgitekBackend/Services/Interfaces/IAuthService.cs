using AgitekBackend.DTOs.Auths;
using AgitekBackend.DTOs.Users;

namespace AgitekBackend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDTO> LoginAsync(SignInDTO dto);
        Task<TokenResponseDTO> RegisterAsync(SignUpDTO dto);
        Task<TokenResponseDTO> VerifyAccountAsync(string token, string userId);
        Task ResendVerifyCodeAsync(string userId);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDTO dto);
        Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<TokenResponseDTO> HandleGoogleLoginAsync(string email, string name);
    }
}
