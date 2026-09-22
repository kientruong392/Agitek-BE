using AgitekBackend.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using RazorLight;

namespace AgitekBackend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly RazorLightEngine _razor;

        public EmailService(IConfiguration config)
        {
            _config = config;

            var templatesPath = Path.Combine(AppContext.BaseDirectory, "Templates", "Emails");
            _razor = new RazorLightEngineBuilder()
                .UseFileSystemProject(templatesPath)
                .SetOperatingAssembly(typeof(Program).Assembly)
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task SendVerificationEmailAsync(string toEmail, string username, string activationCode)
        {
            var model = new { Username = username, ActivationCode = activationCode };
            var html = await _razor.CompileRenderAsync("register.cshtml", model);
            await SendAsync(toEmail, "Xác nhận tài khoản Agitek của bạn", html);
        }

        public async Task SendForgotPasswordEmailAsync(string toEmail, string username, string resetLink)
        {
            var model = new { Username = username, ResetLink = resetLink };
            var html = await _razor.CompileRenderAsync("forgot-password.cshtml", model);
            await SendAsync(toEmail, "Đặt lại mật khẩu Agitek", html);
        }

        private async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _config["EmailSettings:Host"]!;
            var port = int.Parse(_config["EmailSettings:Port"]!);
            var username = _config["EmailSettings:Username"]!;
            var password = _config["EmailSettings:Password"]!;
            var from = _config["EmailSettings:From"]!;
            var enableSsl = bool.Parse(_config["EmailSettings:EnableSsl"] ?? "true");

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            var socketOptions = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(host, port, socketOptions);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
