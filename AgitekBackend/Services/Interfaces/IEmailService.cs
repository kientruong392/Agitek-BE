namespace AgitekBackend.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string username, string activationCode);
        Task SendForgotPasswordEmailAsync(string toEmail, string username, string resetLink);
    }
}
