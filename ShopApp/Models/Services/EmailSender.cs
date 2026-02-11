using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ShopApp.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _configuration["Email:From"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var smtpServer = _configuration["Email:SmtpServer"];
            var port = _configuration["Email:Port"];

            if (string.IsNullOrWhiteSpace(fromEmail) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(port))
            {
                throw new Exception("One or more email settings are missing from appsettings.json");
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = int.Parse(port),
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

    }
}
