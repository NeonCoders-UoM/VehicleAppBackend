using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Vpassbackend.Services
{
    // ✅ Interface
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    // ✅ Implementation
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Force TLS 1.2 for better security and compatibility
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Read SMTP settings from configuration (supports both appsettings.json and environment variables)
                var smtpHost = _config["Smtp:Host"];
                var smtpPort = _config["Smtp:Port"];
                var smtpUsername = _config["Smtp:Username"];
                var smtpPassword = _config["Smtp:Password"];
                var smtpFromEmail = _config["Smtp:FromEmail"];

                // Validate SMTP configuration
                if (string.IsNullOrWhiteSpace(smtpHost) || 
                    string.IsNullOrWhiteSpace(smtpPort) || 
                    string.IsNullOrWhiteSpace(smtpUsername) || 
                    string.IsNullOrWhiteSpace(smtpPassword) || 
                    string.IsNullOrWhiteSpace(smtpFromEmail))
                {
                    _logger.LogError("SMTP configuration is incomplete. Please check environment variables or appsettings.json");
                    throw new InvalidOperationException("SMTP configuration is missing or incomplete");
                }

                _logger.LogInformation($"Attempting to send email to {toEmail} via {smtpHost}:{smtpPort}");

                using var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = int.Parse(smtpPort),
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true,
                    Timeout = 30000 // 30 seconds timeout
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpFromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (SmtpException ex)
            {
                _logger.LogError($"SMTP error while sending email to {toEmail}: {ex.Message} - StatusCode: {ex.StatusCode}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {toEmail}: {ex.Message}");
                throw;
            }
        }
    }
}
