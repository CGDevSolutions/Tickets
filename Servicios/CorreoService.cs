using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Tickets.Servicios
{
    public class CorreoService
    {
        private readonly IConfiguration _config;

        public CorreoService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreo(
            string destinatario,
            string asunto,
            string mensaje)
        {
            var email = new MimeMessage();

            email.From.Add(
                MailboxAddress.Parse(
                    _config["Email:From"]
                )
            );

            email.To.Add(
                MailboxAddress.Parse(destinatario)
            );

            email.Subject = asunto;

            email.Body = new TextPart("html")
            {
                Text = mensaje
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Email:Host"],
                int.Parse(_config["Email:Port"]),
                SecureSocketOptions.SslOnConnect
            );

            await smtp.AuthenticateAsync(
                _config["Email:User"],
                _config["Email:Pass"]
            );

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}