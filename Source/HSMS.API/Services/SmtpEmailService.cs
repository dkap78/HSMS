using HSMS.Services;
using System.Net;
using System.Net.Mail;

namespace HSMS.API
{
    public sealed class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(
                _config["Smtp:Host"],
                int.Parse(_config["Smtp:Port"]!))
                {
                    Credentials = new NetworkCredential(
                    _config["Smtp:Username"],
                    _config["Smtp:Password"]),
                    EnableSsl = true
                };

                var mail = new MailMessage(
                _config["Smtp:From"]!,
                to,
                subject,
                body);

            await client.SendMailAsync(mail);
        }
    }

}
