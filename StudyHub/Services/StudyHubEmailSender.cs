using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace StudyHub.Services
{
    public class StudyHubEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<StudyHubEmailSender> _logger;

        public StudyHubEmailSender(
            IOptions<EmailOptions> options,
            ILogger<StudyHubEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_options.Host) &&
            !string.IsNullOrWhiteSpace(_options.FromEmail);

        public async Task SendAsync(
            string toEmail,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
            {
                _logger.LogInformation(
                    "Email is not configured. Skipped email to {Email}. Subject: {Subject}",
                    toEmail,
                    subject);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
            }

            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
